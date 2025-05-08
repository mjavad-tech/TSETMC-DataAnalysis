using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using static TSETMC_DataAnalysis.MainWindow;

namespace TSETMC_DataAnalysis
{
    public partial class MainWindow : Window
    {
        private int currentPage = 1;
        private const int RowsPerPage = 50; // Number of rows per page
        private List<StockData> fullDataList = new List<StockData>();
        private readonly MyHttpClientService _httpClientService; // Add an instance of MyHttpClientService

        public MainWindow()
        {
            InitializeComponent();
            _httpClientService = new MyHttpClientService(); // Initialize the service

            // Set culture to Persian (Iran)
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fa-IR");
        }

        public class StockData
        {
            public int RowNumber { get; set; }
            public string WebId { get; set; }
            public string TickerCode { get; set; }
            public string Symbol { get; set; }
            public string Name { get; set; }
            public string YesterdayClose { get; set; }
            public string Last { get; set; }
            public string Close { get; set; }
            public string StockPrice { get; set; }
            public string StrikePrice { get; set; }
            public string RemainsDays { get; set; }
            public string CoveredCallProfit { get; set; }
            public string CoveredCallNotApplied { get; set; }
            public string ConversionProfit { get; set; }
            public string MaxLossInLongStraddle { get; set; }
            public string MinProfitInLongStraddle { get; set; }
            public string OpenPosition { get; set; }


        }

        public class MyHttpClientService
        {
            private static readonly HttpClient client;

            static MyHttpClientService()
            {
                client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.91 Safari/537.36");
            }

            public async Task<string> GetWebsiteData()
            {
                const string url = "https://old.tsetmc.com/tsev2/data/MarketWatchPlus.aspx";

                try
                {
                    return await client.GetStringAsync(url);
                }
                catch (HttpRequestException e)
                {
                    // Log or handle the exception
                    Console.WriteLine($"Request error: {e.Message}");
                    return null;
                }
                catch (Exception e)
                {
                    // Handle other potential exceptions
                    Console.WriteLine($"Unexpected error: {e.Message}");
                    return null;
                }

            }

            public async Task<string> GetInstrumentOptionByInstrumentID(string tickerCode)
            {
                string url = $"https://cdn.tsetmc.com/api/Instrument/GetInstrumentOptionByInstrumentID/{tickerCode}";

                try
                {
                    return await client.GetStringAsync(url);
                }
                catch (HttpRequestException e)
                {
                    // Log or handle the exception
                    Console.WriteLine($"Request error: {e.Message}");
                    return null;
                }
                catch (Exception e)
                {
                    // Handle other potential exceptions
                    Console.WriteLine($"Unexpected error: {e.Message}");
                    return null;
                }

            }
        }


        private string NormalizeString(string input)
        {
            // Normalizes Arabic letters to Persian equivalents
            input = input.Replace('ي', 'ی').Replace('ك', 'ک');
            // Normalize presentation forms
            input = input.Normalize(NormalizationForm.FormC);
            return input;
        }

        private async void btn1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingContainer.Visibility = Visibility.Visible;
                progressBar.Visibility = Visibility.Visible;
                loadingIndicator.Visibility = Visibility.Visible;
                progressBar.Value = 0;

                fullDataList.Clear();
                stockDataGrid.ItemsSource = null;
                stockDataGrid.Items.Clear();

                string data = await _httpClientService.GetWebsiteData(); // Use the service to get website data
                if (string.IsNullOrWhiteSpace(data))
                    throw new Exception("Failed to fetch website data.");

                string t = data.Split('@')[2];

                await Task.Run(() =>
                {
                    int rowNumber = 1;
                    foreach (var item in t.Split(';'))
                    {
                        string[] parts = item.Split(',');
                        if (parts.Length > 13)
                        {
                            fullDataList.Add(new StockData
                            {
                                RowNumber = rowNumber++,
                                WebId = parts[0],
                                TickerCode = parts[1],
                                Symbol = NormalizeString(parts[2]), // Normalizing the symbol
                                Name = NormalizeString(parts[3]), // Normalizing the name
                                Close = parts[6],
                                Last = parts[7],
                                YesterdayClose = parts[13],
                            });
                        }
                    }
                });



                var tasks = fullDataList.Select(async stock =>
                {
                    bool keepStock = false;

                    if ((stock.Name.Contains("اختیارخ")) || (stock.Name.Contains("اختیارف")))
                    {
                        string response = await _httpClientService.GetInstrumentOptionByInstrumentID(stock.TickerCode); // Use the service to get instrument option by instrument ID
                        if (response != null)
                        {
                            var instrumentOption = JsonConvert.DeserializeObject<dynamic>(response)?.instrumentOption;

                            if (instrumentOption != null)
                            {
                                double buyOP = instrumentOption.buyOP;
                                if (buyOP != 0)
                                {
                                    stock.OpenPosition = buyOP.ToString();

                                    string endDateStr = instrumentOption.endDate.ToString();
                                    DateTime endDate = DateTime.ParseExact(endDateStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                                    stock.RemainsDays = (endDate - DateTime.Now).Days.ToString();

                                    keepStock = true;
                                }
                            }
                        }

                    }
                    else
                    {
                        stock.RemainsDays = "Not Include";
                        stock.OpenPosition = "Not Include";
                        keepStock = true;
                    }

                    return keepStock ? stock : null;
                });

                fullDataList = (await Task.WhenAll(tasks)).Where(stock => stock != null).ToList();


                Parallel.ForEach(fullDataList, y =>
                {
                    int c = 0;

                    if (y.Name.Contains("اختیارخ"))
                    {
                        // Replace the first character of y.Symbol from "ض" to "ط"
                        string buyoption = y.Symbol;
                        string selloption = "ط" + buyoption.Substring(1);

                        string[] nameParts = y.Name.Split(new[] { '-' }, 3); // Split into 3 parts
                        if (nameParts.Length == 3)
                        {
                            y.StrikePrice = nameParts[1];
                            string baseStockName = nameParts[0].Replace("اختیارخ", "").Trim(); // Remove "اختیارخ" and trim

                            var baseStock = fullDataList.FirstOrDefault(stock => stock.Symbol.Equals(baseStockName, StringComparison.OrdinalIgnoreCase));
                            if (baseStock != null)
                            {
                                y.StockPrice = baseStock.Close;

                                if (double.TryParse(baseStock.Close, out double stockPrice) &&
                                    double.TryParse(y.StrikePrice, out double strikePrice) &&
                                    double.TryParse(y.Close, out double closePrice))
                                {
                                    double coveredCallProfit = ((strikePrice - stockPrice + closePrice) / stockPrice) * 100;
                                    y.CoveredCallProfit = coveredCallProfit.ToString("F2") + "%";
                                    double CoveredCallNotApplied = (closePrice / stockPrice) * 100;
                                    y.CoveredCallNotApplied = CoveredCallNotApplied.ToString("F2") + "%";

                                    // Find the buyoption row for symbol "ط" + baseStockName
                                    var SellOptionRow = fullDataList.FirstOrDefault(stock => stock.Symbol.Equals(selloption, StringComparison.OrdinalIgnoreCase));
                                    if (SellOptionRow != null)
                                    {
                                        if (double.TryParse(SellOptionRow.Close, out double SellOptionClosePrice))
                                        {
                                            c++;

                                            // Calculate Conversion Profit
                                            double conversionProfit = ((closePrice - SellOptionClosePrice + strikePrice - stockPrice) / (stockPrice + SellOptionClosePrice) * 100);
                                            y.ConversionProfit = conversionProfit.ToString("F2") + "%";
                                            SellOptionRow.ConversionProfit = conversionProfit.ToString("F2") + "%";
                                            SellOptionRow.StrikePrice = y.StrikePrice;
                                            SellOptionRow.StockPrice = y.StockPrice;

                                            //Calculate max Straddle loss 
                                            double MaxLossInLongStraddle = closePrice + SellOptionClosePrice;
                                            y.MaxLossInLongStraddle = MaxLossInLongStraddle.ToString();
                                            SellOptionRow.MaxLossInLongStraddle = MaxLossInLongStraddle.ToString();

                                            //Calculate min Straddle profit 
                                            double MinProfitInLongStraddle = (((strikePrice - stockPrice - closePrice - SellOptionClosePrice) / (closePrice + SellOptionClosePrice)) * 100);
                                            y.MinProfitInLongStraddle = MinProfitInLongStraddle.ToString("F2") + "%";
                                            SellOptionRow.MinProfitInLongStraddle = MinProfitInLongStraddle.ToString("F2") + "%";
                                        }
                                        else
                                        {
                                            y.ConversionProfit = "Not Include";
                                            y.MaxLossInLongStraddle = "Not Include";
                                            y.MinProfitInLongStraddle = "Not Include";
                                        }
                                    }
                                    else
                                    {
                                        y.ConversionProfit = "Not Include";
                                        y.MaxLossInLongStraddle = "Not Include";
                                        y.MinProfitInLongStraddle = "Not Include";
                                    }
                                }
                                else
                                {
                                    y.CoveredCallProfit = "Not Include";
                                    y.CoveredCallNotApplied = "Not Include";
                                    y.ConversionProfit = "Not Include";
                                    y.MaxLossInLongStraddle = "Not Include";
                                    y.MinProfitInLongStraddle = "Not Include";
                                }
                            }
                            else
                            {
                                y.StockPrice = "Not Include";
                                y.CoveredCallProfit = "Not Include";
                                y.CoveredCallNotApplied = "Not Include";
                                y.ConversionProfit = "Not Include";
                                y.MaxLossInLongStraddle = "Not Include";
                                y.MinProfitInLongStraddle = "Not Include";
                            }
                        }
                    }
                    else
                    {
                        if ((c == 0) && (y.Name.Contains("اختیارف")))
                        {
                            y.CoveredCallProfit = "Not Include";
                            y.CoveredCallNotApplied = "Not Include";
                            y.ConversionProfit = "Not Include";
                            y.MaxLossInLongStraddle = "Not Include";
                            y.MinProfitInLongStraddle = "Not Include";

                        }
                        else if ((c != 0) || (y.Name.Contains("اختیارف")))
                        {

                            y.CoveredCallProfit = "Not Include";
                            y.CoveredCallNotApplied = "Not Include";
                        }
                        else
                        {
                            y.StrikePrice = "Not Include";
                            y.StockPrice = "Not Include";
                            y.CoveredCallProfit = "Not Include";
                            y.CoveredCallNotApplied = "Not Include";
                            y.ConversionProfit = "Not Include";
                            y.MaxLossInLongStraddle = "Not Include";
                            y.MinProfitInLongStraddle = "Not Include";
                        }

                    }
                });


                int totalPages = (int)Math.Ceiling((double)fullDataList.Count / RowsPerPage);
                currentPage = 1;
                UpdatePageInfo(totalPages);
                DisplayCurrentPage();

                // Display date in Persian calendar
                PersianCalendar persianCalendar = new PersianCalendar();
                DateTime now = DateTime.Now;
                string persianDate = persianCalendar.GetYear(now) + "/" +
                                     persianCalendar.GetMonth(now).ToString("00") + "/" +
                                     persianCalendar.GetDayOfMonth(now).ToString("00") + " " +
                     now.ToString("HH:mm:ss");

                date.Text = "امروز: " + persianDate;
                date.Foreground = Brushes.Blue; // Change the text color to blue for better visibility

                progressBar.Visibility = Visibility.Collapsed;
                loadingIndicator.Visibility = Visibility.Collapsed;
                loadingContainer.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                loadingIndicator.Visibility = Visibility.Collapsed;
                loadingContainer.Visibility = Visibility.Collapsed;
                MessageBox.Show("Error fetching data: " + ex.Message);
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)fullDataList.Count / RowsPerPage);
            if (currentPage < totalPages)
            {
                currentPage++;
                DisplayCurrentPage();
                UpdatePageInfo(totalPages);
            }
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                DisplayCurrentPage();
                UpdatePageInfo((int)Math.Ceiling((double)fullDataList.Count / RowsPerPage));
            }
        }

        private void DisplayCurrentPage()
        {
            int startRow = (currentPage - 1) * RowsPerPage;
            int endRow = startRow + RowsPerPage;

            stockDataGrid.ItemsSource = fullDataList.Skip(startRow).Take(RowsPerPage).ToList();
        }

        private void UpdatePageInfo(int totalPages)
        {
            pageInfo.Text = $"Page {currentPage} of {totalPages}";
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = searchBox.Text.ToLower();

            var filteredList = fullDataList.Where(stock =>
                stock.Name.ToLower().Contains(searchText) ||
                stock.Symbol.ToLower().Contains(searchText)).ToList();

            stockDataGrid.ItemsSource = filteredList;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text == "Name or Symbol")
            {
                searchBox.Text = "";
                searchBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                searchBox.Text = "Name or Symbol";
                searchBox.Foreground = Brushes.Gray;
            }
        }

    }
}
