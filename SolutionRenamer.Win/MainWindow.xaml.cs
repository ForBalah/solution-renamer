using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using SolutionRenamer.Win.Logic;
using SolutionRenamer.Win.Logic.Events;
using SolutionRenamer.Win.Logic.FileSystem;
using SolutionRenamer.Win.Logic.Logging;
using SolutionRenamer.Win.Logic.Shell;

namespace SolutionRenamer.Win
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Renamer _renamer;
        private DispatcherTimer _renameTimer = new DispatcherTimer();
        private RenamerRule _ruleSet;

        public MainWindow()
        {
            InitializeComponent();
        }

        private RenamerRule BuildRuleSet()
        {
            return new RenamerRuleBuilder()
                .SetNext(RenamerRule.IsNotGit)
                .SetNext(RenamerRule.IsNotDll)
                .SetNext(RenamerRule.IsNotBinOrObj)
                .SetNext(RenamerRule.IsNotNuget)
                .SetNext(RenamerRule.IsNotDbmdl)
                .SetNext(RenamerRule.IsNotCache)
                .SetNext(RenamerRule.IsNotPdb)
                .SetNext(RenamerRule.IsNotSuo)
                .Build();
        }

        private void CleanBinFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clean up the bin and obj folders?", "Clean folders", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CleanBinObjFolders();
            }
        }

        private void CleanBinObjFolders()
        {
            var success = _renamer.CleanFolders("bin") &&
                _renamer.CleanFolders("obj");

            if (!success)
            {
                MessageBox.Show("Error occured during cleaning.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteDbFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var deleteFolderName = ConfigurationManager.AppSettings["DatabaseFolder"];
            if (MessageBox.Show($"This will delete the MDF/LDF files from {deleteFolderName}. Are you sure?", "Delete Local DB files", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var result = _renamer.CleanDatabaseFiles(ConfigurationManager.AppSettings["DatabaseFolder"]);
                if (result)
                {
                    MessageBox.Show("LocalDB files deleted successfully.", "Success");
                }
                else
                {
                    MessageBox.Show("Could not delete Local DB files.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Renaming cannot be undone. Proceed?", "Attention!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                LogsTabItem.Focus();
                var targetFolder = _renamer.TargetFolder;
                var progressWindow = new ProgressWindow();
                progressWindow.Topmost = true;
                progressWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                progressWindow.Owner = this;

                var progressReport = new Progress<RenamerProgress>(p =>
                {
                    progressWindow.ProgressTextBlock.Text = p.Message;
                    progressWindow.RenamerProgressBar.Value = p.Percentage * 100d;

                    if (p.Logger != null)
                    {
                        if (p.IsError)
                        {
                            p.Logger.WriteWarning(p.Message);
                        }
                        else
                        {
                            p.Logger.WriteInfo(p.Message);
                        }
                    }
                });
                progressWindow.Show();
                IsEnabled = false;

                try
                {
                    await Task.Run(() => _renamer.DoRename(progressReport));

                    progressWindow.Hide();

                    if (_renamer.Completed)
                    {
                        if (MessageBox.Show("Renaming complete! Do you want to clean the bin and obj folders?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            CleanBinObjFolders();
                        }

                        MessageBox.Show("Renaming complete! A log file will be created on the file system.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Problems were encountered. See the log tab for more information.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    IsEnabled = true;
                    progressWindow.Close();
                    ExecuteButton.IsEnabled = _renamer.Ready;

                    // TODO: this should go into the filesystem class intead.
                    File.WriteAllText(Path.Combine(Path.GetDirectoryName(targetFolder), "RenamerLog.txt"), LogsTextBox.Text);
                }

                LogsTextBox.ScrollToLine(LogsTextBox.LineCount - 1);
            }
        }

        private void RecursiveShowFiles(RenamerInfo root, ItemCollection items)
        {
            var label = new Label { Content = root.ToString() };
            label.Foreground = root.IsIncluded ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Gray);
            var treeViewItem = new TreeViewItem
            {
                Header = label,
                IsExpanded = root.IsIncluded
            };

            items.Add(treeViewItem);

            foreach (var child in root.Children.OrderBy(i => i.FileType != FileType.Directory))
            {
                RecursiveShowFiles(child, treeViewItem.Items);
            }
        }

        private void Renamer_RenamesSet(object sender, RenamesSetEventArgs e)
        {
            ExecuteButton.IsEnabled = _renamer.Ready;
        }

        private void Renamer_SolutionRenamed(object sender, SolutionRenamedEventArgs e)
        {
            // do nothing.. for now.
        }

        private void Renamer_TargetFolderAdded(object sender, TargetFolderAddedEventArgs e)
        {
            SolutionFolderLabel.Content = e.Path;
            FromSolutionNameTextBox.IsEnabled = e.IsValid;
            ToSolutionNameTextBox.IsEnabled = e.IsValid;

            if (e.IsValid)
            {
                var solutionPath = _renamer.Root.Children.First(i => i.FileType == FileType.Solution).Path;
                FromSolutionNameTextBox.Text = Path.GetFileNameWithoutExtension(solutionPath);

                RecursiveShowFiles(_renamer.Root, FilesTreeView.Items);
            }
            else
            {
                FilesTreeView.Items.Clear();
            }

            ExecuteButton.IsEnabled = _renamer.Ready;
        }

        private void RenameTimer_Tick(object sender, EventArgs e)
        {
            _renamer.SetRename(FromSolutionNameTextBox.Text, ToSolutionNameTextBox.Text);
            _renameTimer.Stop();
            ExecuteButton.IsEnabled = _renamer.Ready;
        }

        private void SolutionFolderSelectButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                _renamer.SetTargetFolder(dialog.FileName);
            }
        }

        private void SolutionNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            ExecuteButton.IsEnabled = false;
        }

        private void SolutionNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _renamer.SetRename(string.Empty, string.Empty);
            _renameTimer.Stop();
            _renameTimer.Start();
        }

        private void StartLocalDbButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to restart the SQL local DB instance?", "Restart local DB", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var result = ProcessUtility.RunCommand("Start SQL LocalDB", @"sqllocaldb start MSSQLLocalDB", TextBoxLogger.Get());
                if (result.IsSuccess)
                {
                    MessageBox.Show("SQL LocalDB started successfully.", "Success");
                }
                else
                {
                    MessageBox.Show("Could not successfully start SQL Local DB.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            TextBoxLogger.Initialize(LogsTextBox, StatusLabel, StatusImage);

            _ruleSet = BuildRuleSet();
            _renamer = new Renamer(TextBoxLogger.Get(), FileSystemWrapper.Create(), _ruleSet);
            _renamer.TargetFolderAdded += Renamer_TargetFolderAdded;
            _renamer.RenamesSet += Renamer_RenamesSet;
            _renamer.SolutionRenamed += Renamer_SolutionRenamed;

            _renameTimer.Tick += RenameTimer_Tick;
            _renameTimer.Interval = TimeSpan.FromSeconds(1.5);
        }
    }
}