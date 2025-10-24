using LibGit2Sharp;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace merge_vsix
{
    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class ToolWindow1Control : UserControl
    {

        readonly string repoPath = "";
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Control"/> class.
        /// </summary>
        public ToolWindow1Control()
        {
            this.InitializeComponent();

            repoPath = @"C:\Users\p\source\repos\merge_vsix"; // путь к локальному репозиторию

            LoadBranches(repoPath);
        }

        private void LoadBranches(string repoPath)
        {
            if (!Repository.IsValid(repoPath)) return;


            using (var repo = new Repository(repoPath))
            {
                // Подтягиваем последние remote ветки
                var remote = repo.Network.Remotes["origin"];

                Commands.Fetch(repo, remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), null, "");

                // Локальные ветки
                var localBranches = repo.Branches
                                        //.Where(b => !b.IsRemote)
                                        .Select(b => b.FriendlyName)
                                        .ToList();
                
                FromBranchCombo.ItemsSource = localBranches;
                ToBranchCombo.ItemsSource = localBranches;




            }
        }


        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();

            string fromBranch = FromBranchCombo.SelectedItem as string;
            string toBranch = ToBranchCombo.SelectedItem as string;

            if (string.IsNullOrEmpty(fromBranch) || string.IsNullOrEmpty(toBranch))
                return;

            await MergeBranchesAsync(fromBranch, toBranch);
        }

        private async Task MergeBranchesAsync(string from, string to)
        {
            //11
            using (var repo = new Repository(repoPath))
            {
                var previousBranchName = repo.Head.FriendlyName;

                var stash = repo.Stashes.Add(repo.Config.BuildSignature(DateTimeOffset.Now), "Auto-stash");

                Commands.Checkout(repo, repo.Branches[to]);

                var remote = repo.Network.Remotes["origin"];
                Commands.Fetch(repo, remote.Name, new string[] { from }, new FetchOptions(), null);

                var fromBranch = repo.Branches[from];

                var mergeResult = repo.Merge(fromBranch, repo.Config.BuildSignature(DateTimeOffset.Now),
                    new MergeOptions { FileConflictStrategy = CheckoutFileConflictStrategy.Normal });

                Commands.Checkout(repo, repo.Branches[previousBranchName]);

                if (repo.Stashes.Count() > 0)
                    repo.Stashes.Pop(0, new StashApplyOptions { ApplyModifiers = StashApplyModifiers.Default });

                // --- Статус в StatusBar ---
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var statusBar = Package.GetGlobalService(typeof(SVsStatusbar)) as IVsStatusbar;

                if (mergeResult.Status == MergeStatus.Conflicts)
                    statusBar?.SetText($"Merge конфликт! {from} → {to}");
                else
                    statusBar?.SetText($"Merge успешно: {from} → {to}");
            }
        }

    }
}