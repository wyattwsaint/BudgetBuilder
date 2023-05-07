using IronOcr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace BudgetBuilder
{
    public class BankTransaction : INotifyPropertyChanged
    {

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        protected bool Set<T>(ref T location, T value, [CallerMemberName] string? propertyName = null)
        {
            if (RuntimeHelpers.Equals(location, value)) return false;
            location = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private decimal house;
        public decimal House
        {
            get => house;
            set => Set(ref house, value);
        }
        private decimal transportation;
        public decimal Transportation
        {
            get => transportation;
            set => Set(ref transportation, value);
        }
        private decimal food;
        public decimal Food
        {
            get => food;
            set => Set(ref food, value);
        }
        private decimal utilities;
        public decimal Utilities
        {
            get => utilities;
            set => Set(ref utilities, value);
        }
        private decimal clothing;
        public decimal Clothing
        {
            get => clothing;
            set => Set(ref clothing, value);
        }
        private decimal medical;
        public decimal Medical
        {
            get => medical;
            set => Set(ref medical, value);
        }
        private decimal insurance;
        public decimal Insurance
        {
            get => insurance;
            set => Set(ref insurance, value);
        }
        private decimal houseSupplies;
        public decimal HouseSupplies
        {
            get => houseSupplies;
            set => Set(ref houseSupplies, value);
        }
        private decimal personal;
        public decimal Personal
        {
            get => personal;
            set => Set(ref personal, value);
        }
        private decimal debt;
        public decimal Debt
        {
            get => debt;
            set => Set(ref debt, value);
        }
        private decimal retirement;
        public decimal Retirement
        {
            get => retirement;
            set => Set(ref retirement, value);
        }
        private decimal education;
        public decimal Education
        {
            get => education;
            set => Set(ref education, value);
        }
        private decimal savings;
        public decimal Savings
        {
            get => savings;
            set => Set(ref savings, value);
        }
        private decimal giving;
        public decimal Giving
        {
            get => giving;
            set => Set(ref giving, value);
        }
        private decimal entertainment;
        public decimal Entertainment
        {
            get => entertainment;
            set => Set(ref entertainment, value);
        }

        public BankTransaction()
        {
        }

        private ObservableCollection<TransactionLineItem> transactions;
        public ObservableCollection<TransactionLineItem> Transactions
        {
            get { return transactions; }
            set
            {
                if (value != transactions)
                {
                    transactions = value;
                    OnPropertyChanged(nameof(Transactions));
                }
            }
        }

        public void Parse(string? file)
        {
            ClearCategoryTotals();

            var tags = SqliteDataAccess.Load();

            var ocr = new IronTesseract();
            using (OcrInput input = new OcrInput())
            {
                // We can also select specific PDF page numnbers to OCR
                input.AddPdf($"{file}", "password");
                OcrResult result = ocr.Read(input);

                Transactions = new ObservableCollection<TransactionLineItem>();
                var transactionBuilder = new ObservableCollection<TransactionLineItem>();

                var transactionCounter = 0;
                var arrRawData = result.Text.Split("\r");
                foreach (var item in arrRawData)
                {
                    string pattern = @"\d\d\/\d\d.+\d+\.\d{2}$";
                    Match m = Regex.Match(item, pattern, RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        var rawComponents = item.Split(" ");

                        var components = new List<string>();
                        foreach (var i in rawComponents)
                        {
                            string datePattern = @"\(\d\d\/\d\d\/\d\d\d\d\)$";
                            Match match = Regex.Match(i, datePattern, RegexOptions.IgnoreCase);
                            if (match.Success) continue;

                            components.Add(i);
                        }

                        decimal.TryParse(components[components.Count - 2], out var stringResult);
                        
                        if (stringResult == default) continue;

                        DateOnly.TryParse(components[0], out var date);
                        
                        var description = String.Join(" ", components.ToArray(), 1, components.Count - 3);
                        
                        decimal.TryParse(components[components.Count - 2], out var amount);

                        var category = tags.Where(t => t.Description == description).FirstOrDefault().Category;

                        transactionBuilder.Add(new TransactionLineItem(date, description, amount, category, transactionCounter));

                        transactionCounter++;
                    }
                }
                Transactions = new ObservableCollection<TransactionLineItem>(transactionBuilder.OrderBy(tb => tb.Date).ThenBy(tb => tb.Count));
                TallyTotalsByCategory(Transactions);
            }
        }

        public void TallyTotalsByCategory(ObservableCollection<TransactionLineItem> transactions)
        {
            foreach (TransactionLineItem item in transactions)
            {
                if (item.Category is not null)
                {
                    switch (item.Category)
                    {
                        case "House":
                            House += item.Amount;
                            break;
                        case "Transportation":
                            Transportation += item.Amount;
                            break;
                        case "Food":
                            Food += item.Amount;
                            break;
                        case "Utilities":
                            Utilities += item.Amount;
                            break;
                        case "Clothing":
                            Clothing += item.Amount;
                            break;
                        case "Medical":
                            Medical += item.Amount;
                            break;
                        case "Insurance":
                            Insurance += item.Amount;
                            break;
                        case "HouseSupplies":
                            HouseSupplies += item.Amount;
                            break;
                        case "Personal":
                            Personal += item.Amount;
                            break;
                        case "Debt":
                            Debt += item.Amount;
                            break;
                        case "Retirement":
                            Retirement += item.Amount;
                            break;
                        case "Education":
                            Education += item.Amount;
                            break;
                        case "Savings":
                            Savings += item.Amount;
                            break;
                        case "Giving":
                            Giving += item.Amount;
                            break;
                        case "Entertainment":
                            Entertainment += item.Amount;
                            break;
                    }
                }
            }
        }

        private void ClearCategoryTotals()
        {
            House = 0;
            Transportation = 0;
            Food = 0;
            Utilities = 0;
            Clothing = 0;
            Medical = 0;
            Insurance = 0;
            HouseSupplies = 0;
            Personal = 0;
            Debt = 0;
            Retirement = 0;
            Education = 0;
            Savings = 0;
            Giving = 0;
            Entertainment = 0;
        }
    }
}
