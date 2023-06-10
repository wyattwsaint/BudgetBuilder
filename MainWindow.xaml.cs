using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BudgetBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Height = SystemParameters.PrimaryScreenHeight * 0.95;
            DataContext = new BankTransaction();
        }


        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var mi = (MenuItem)sender;
            var category = (string)mi.Header;

            var menuItem = (MenuItem)sender;

            var contextMenu = (ContextMenu)menuItem.Parent;

            var item = (DataGrid)contextMenu.PlacementTarget;

            var transactionLineItem = (TransactionLineItem)item.SelectedCells[0].Item;

            SqliteDataAccess.Save(transactionLineItem.Description, category);

            var transaction = (BankTransaction)DataContext;

            var updatedTransactionToAdd = transaction.Transactions!.Where(t => t.Description == transactionLineItem.Description)
                .Select(t => new TransactionLineItem(transactionLineItem.Date, transactionLineItem.Description, transactionLineItem.Amount, category, transactionLineItem.Count)).First();
            var transactionToAdjustPreviousTotal = transactionLineItem with { Amount = -transactionLineItem.Amount };

            transaction.Transactions!.Remove(transactionLineItem);

            transaction.Transactions.Add(updatedTransactionToAdd);

            var updateTotal = new ObservableCollection<TransactionLineItem>();
            updateTotal.Add(updatedTransactionToAdd);
            updateTotal.Add(transactionToAdjustPreviousTotal);
            transaction.TallyTotalsByCategory(updateTotal);
            transaction.Transactions = new ObservableCollection<TransactionLineItem>(transaction.Transactions.OrderBy(tb => tb.Date).ThenBy(tb => tb.Count));
        }

        private async void MyList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                var transaction = (BankTransaction)DataContext;

                var progressBar = new BudgetBuilder.ProgressBar();

                progressBar.Show();
                await Task.Run(() =>
                {
                    transaction.Parse(file: files[0]);
                });
                progressBar.Hide();
            }
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if ((string)e.Column.Header is "Count" or "Category")
            {
                e.Cancel = true;
            }
        }
    }
}
