using LibGit2Sharp;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace merge_vsix
{
    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class ToolWindow1Control : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Control"/> class.
        /// </summary>
        public ToolWindow1Control()
        {
            this.InitializeComponent();

            string repoPath = @"C:\Users\p\source\repos\Aggregator"; // путь к локальному репозиторию

            LoadBranches(repoPath);
        }

        private void LoadBranches(string repoPath)
        {
            if (!Repository.IsValid(repoPath)) return;


            using (var repo = new Repository(repoPath))
            {
                var localBranches = repo.Branches
                                 .Where(b => !b.IsRemote)
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
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "ToolWindow1");
        }
    }
}