using JnvlsList.Model;
using JnvlsList.Models;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Collections.ObjectModel;
using Color = System.Windows.Media.Color;
using System.Threading;

namespace ParserExcel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string pricePath;
        string allowancePath;
        static string header;
        List<String> Regions = new List<string>(new string[] { "Владимирская область","Республика Башкортостан","Ивановская область","Иркутская область","Калининградская область","Краснодарский край","Красноярский край",
        "г.Москва","Московская область","Нижегородская область","Новгородская область","Омская область","Орловская область","Приморский край","Ростовская область","Республика Саха (Якутия)","Свердловская область","Тверская область","Республика Хакасия"});
        //List<String> Regions = new List<string>(new string[] { "Краснодарский край" });
        List<Allowance> ListAllowance = new List<Allowance>();
        List<string> excelData = new List<string>();
        List<string> excelDataExept = new List<string>();
        List<Drug> exceptionsDrugList = new List<Drug>();
        List<Drug> VladimirskayaObl = new List<Drug>();
        List<Drug> Bashkiriya = new List<Drug>();
        List<Drug> IvanovskayaObl = new List<Drug>();
        List<Drug> IrkutskayaObl = new List<Drug>();
        List<Drug> KaliningradskayaObl = new List<Drug>();
        List<Drug> Krasnodarskii = new List<Drug>();
        List<Drug> Krasnoyarskii = new List<Drug>();
        List<Drug> Moskva = new List<Drug>();
        List<Drug> MO = new List<Drug>();
        List<Drug> NijnegorodskayaObl = new List<Drug>();
        List<Drug> NovgorodskayaObl = new List<Drug>();
        List<Drug> OmskayaObl = new List<Drug>();
        List<Drug> OrlovskayaObl = new List<Drug>();
        List<Drug> Primorskii = new List<Drug>();
        List<Drug> RostovskayaObl = new List<Drug>();
        List<Drug> Saha = new List<Drug>();
        List<Drug> Sverdlovskaya = new List<Drug>();
        List<Drug> tverskaya = new List<Drug>();
        List<Drug> Hakasiya = new List<Drug>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DropBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                var listbox = sender as Canvas;
                listbox.Background = new SolidColorBrush(Color.FromRgb(155, 155, 155));
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DropBox_DragLeave(object sender, DragEventArgs e)
        {
            var listbox = sender as Canvas;
            listbox.Background = new SolidColorBrush(Color.FromRgb(226, 226, 226));
        }

        private void DropBox_Drop(object sender, DragEventArgs e)
        {         

            //Regex regexPrice = new Regex(@"Перечень предельных оптовых и розничных цен(\w*)");
            //Regex regexAllowance = new Regex(@"Предельные_размеры(\w*)");
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);                                                      
                    
                var fi1 = new FileInfo(files.First());
                long length = new System.IO.FileInfo(fi1.FullName).Length;
                if (length > 3000000)
                {
                    pricePath = files.First();
                    TextBoxOne.Text = pricePath;
                }
                else if(length < 3000000)
                {
                    allowancePath = files.First();
                    TextBoxTwo.Text = allowancePath;
                }


                //foreach
                //MatchCollection matches = regexAllowance.Matches(filePath);
                //if (matches.Count > 0)
                //    allowancePath = filePath;
                //else
                //    pricePath = filePath;
                //MatchCollection matches1 = regexPrice.Matches(filePath);
                //if (matches1.Count > 0)
                //    pricePath = filePath;
                //else
                //    allowancePath = filePath;

            }

            var listbox = sender as Canvas;
            listbox.Background = new SolidColorBrush(Color.FromRgb(226, 226, 226));
        }


        protected void TasksToRun()
        {
            try
            {
                //read from drug excel
                Task task1 = new Task(() => ReadDrugsFromExcel());
                task1.Start();
                task1.Wait();
                //send drug list
                Task<List<Drug>> task2 = new Task<List<Drug>>(() => DeserializeExcel(excelData));
                task2.Start();
                task2.Wait();
                //Send exceptions list
                Task<List<Drug>> task3 = new Task<List<Drug>>(() => DeserializeExcel(excelDataExept));
                task3.Start();
                task3.Wait();
                exceptionsDrugList = new List<Drug>(task3.Result);
                //Create Lists of all regions
                Task task4 = new Task(() => CreateSortedRegionLists(task2.Result));
                task4.Start();
                task4.Wait();
                //Run read allowances
                Task task5 = new Task(() => ReadDotationsFromExcel());
                task5.Start();
                task5.Wait();
                //Run Calculate dotations
                Task task6 = new Task(() => CalculateDotation());
                task6.Start();
                task6.Wait();

                Task task7 = new Task(() => ClearAllLists());
                task7.Start();
                task7.Wait();

                Thread t = new Thread(new ThreadStart(CloseProgressBar));
                t.Start();
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void CloseProgressBar()
        {
            Dispatcher.Invoke(() => {
                // Code causing the exception or requires UI thread access
                label.Content = "Finished";
                ProgressBar.IsIndeterminate = false;
            });
            
        }

        void ShowProgressBar()
        {            
            ProgressBar.IsIndeterminate = true;                
        }

        async void RunEveryThing()
        {
            await Task.Run(() => TasksToRun());
        } 

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e){}

        private async void One_Click(object sender, RoutedEventArgs e)
        {            
            ShowProgressBar();
            await Task.Run(() => RunEveryThing());
                      
        }

        public void CreateExcelReport(List<Drug> listDrugs, string fileName)
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                List<Drug> listByFirstW = new List<Drug>();
                //listDrugs.AddRange(exceptionsDrugList);
                using (ExcelPackage excel = new ExcelPackage())
                {
                    for (int i = 0; i < listDrugs.Count - 1; i++)
                    {
                        listByFirstW.Add(listDrugs[i]);

                        if (listDrugs[i].Name.First() != listDrugs[i + 1].Name.First() || listDrugs[i + 1] == listDrugs.Last())
                        {
                            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add(listDrugs[i].Name.First().ToString());

                            worksheet.Cells[1, 1].Value = header;

                            worksheet.Cells["A2"].Value = "Торговое наименование лекарственного препарата";
                            worksheet.Cells["B2"].Value = "МНН";
                            worksheet.Cells[2, 3].Value = "Лекарственная форма, дозировка, упаковка (полная)";
                            worksheet.Cells[2, 4].Value = "Владелец РУ/производитель/упаковщик/Выпускающий контроль";
                            worksheet.Cells[2, 5].Value = "Код АТХ";
                            worksheet.Cells[2, 6].Value = "Количество в потреб.упаковке";
                            worksheet.Cells[2, 7].Value = "Цена указана для первич. упаковки";
                            worksheet.Cells[2, 8].Value = "№ РУ";
                            worksheet.Cells[2, 9].Value = "Дата регистрации цены(№ решения)";
                            worksheet.Cells[2, 10].Value = "Штрих-код (EAN13)";
                            worksheet.Cells[2, 11].Value = "Предельная цена руб. без НДС";
                            worksheet.Cells[2, 12].Value = "Предельная цена руб. с НДС";
                            worksheet.Cells[2, 13].Value = "Предельная оптовая надбавка, руб.*";
                            worksheet.Cells[2, 14].Value = "Предельная оптовая цена, руб., (без НДС)*";
                            worksheet.Cells[2, 15].Value = "Предельная оптовая цена руб., (с НДС)*";
                            worksheet.Cells[2, 16].Value = "Предельная розничная надбавка, руб.*";
                            worksheet.Cells[2, 17].Value = "Предельная розничная цена,  руб. (без НДС)*";
                            worksheet.Cells[2, 18].Value = "Предельная розничная цена,  руб. (с НДС)*";
                            worksheet.Cells.Style.WrapText = true;
                            worksheet.Cells["A3"].LoadFromCollection<Drug>(listByFirstW);

                            worksheet.Cells["A1:R1"].Merge = true;
                            for (int k = worksheet.Dimension.Start.Row; k <= worksheet.Dimension.End.Row; k++)
                            {
                                worksheet.Row(k).Style.Font.Size = 8;
                                worksheet.Row(k).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                                worksheet.Row(k).Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                worksheet.Row(k).Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                worksheet.Row(k).Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                ////loop all columns in a row
                                //for (int j = 1; j <= 18; j++)
                                //{
                                //    worksheet.Column(j).Style.Font.Size = 8;
                                //    worksheet.Column(j).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                                //    worksheet.Cells[k, j].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                //    worksheet.Cells[k, j].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                //    worksheet.Cells[k, j].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                                //}
                            }
                            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                            //Row1
                            worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Row(1).Height = 62.25;
                            worksheet.Row(1).Style.Font.Bold = true;
                            worksheet.Row(1).Style.Font.Size = 14;
                            worksheet.Row(1).Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.None;
                            worksheet.Row(1).Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.None;
                            worksheet.Row(1).Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.None;
                            //Row2
                            worksheet.Cells["B2:R2"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells["B2:R2"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells["B2:R2"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells["B2:R2"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Row(2).Style.Font.Size = 8;
                            worksheet.Row(2).Style.Font.Bold = true;
                            worksheet.Row(2).Height = 63;
                            worksheet.Row(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            worksheet.Row(2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            worksheet.Row(2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Row(2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            worksheet.Column(1).Width = 22.14;
                            worksheet.Column(2).Width = 18;
                            worksheet.Column(3).Width = 23.43;
                            worksheet.Column(4).Width = 25.71;
                            worksheet.Column(6).Width = 8.43;
                            worksheet.Column(7).Width = 8.43;
                            worksheet.Column(9).Width = 10.71;
                            worksheet.Column(10).Width = 11;
                            worksheet.Column(11).Width = 8.43;
                            worksheet.Column(12).Width = 8.43;
                            worksheet.Column(13).Width = 8.43;
                            worksheet.Column(14).Width = 8.43;
                            worksheet.Column(15).Width = 8.43;
                            worksheet.Column(16).Width = 8.43;
                            worksheet.Column(17).Width = 8.43;
                            worksheet.Column(18).Width = 8.43;
                            worksheet.Column(6).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                            listByFirstW.Clear();
                        }

                    }
                    ExcelWorksheet worksheet1 = excel.Workbook.Worksheets.Add("Исключенные");

                    worksheet1.Cells[1, 1].Value = header;

                    worksheet1.Cells["A2"].Value = "Торговое наименование лекарственного препарата";
                    worksheet1.Cells["B2"].Value = "МНН";
                    worksheet1.Cells[2, 3].Value = "Лекарственная форма, дозировка, упаковка (полная)";
                    worksheet1.Cells[2, 4].Value = "Владелец РУ/производитель/упаковщик/Выпускающий контроль";
                    worksheet1.Cells[2, 5].Value = "Код АТХ";
                    worksheet1.Cells[2, 6].Value = "Количество в потреб.упаковке";
                    worksheet1.Cells[2, 7].Value = "Цена указана для первич. упаковки";
                    worksheet1.Cells[2, 8].Value = "№ РУ";
                    worksheet1.Cells[2, 9].Value = "Дата регистрации цены(№ решения)";
                    worksheet1.Cells[2, 10].Value = "Штрих-код (EAN13)";
                    worksheet1.Cells[2, 11].Value = "Предельная цена руб. без НДС";
                    worksheet1.Cells[2, 12].Value = "Предельная цена руб. с НДС";
                    worksheet1.Cells[2, 13].Value = "Предельная оптовая надбавка, руб.*";
                    worksheet1.Cells[2, 14].Value = "Предельная оптовая цена, руб., (без НДС)*";
                    worksheet1.Cells[2, 15].Value = "Предельная оптовая цена руб., (с НДС)*";
                    worksheet1.Cells[2, 16].Value = "Предельная розничная надбавка, руб.*";
                    worksheet1.Cells[2, 17].Value = "Предельная розничная цена,  руб. (без НДС)*";
                    worksheet1.Cells[2, 18].Value = "Предельная розничная цена,  руб. (с НДС)*";
                    worksheet1.Cells.Style.WrapText = true;
                    worksheet1.Cells["A3"].LoadFromCollection<Drug>(exceptionsDrugList);

                    worksheet1.Cells["A1:R1"].Merge = true;
                    for (int k = worksheet1.Dimension.Start.Row; k <= worksheet1.Dimension.End.Row; k++)
                    {
                        worksheet1.Row(k).Style.Font.Size = 8;
                        worksheet1.Row(k).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Top;
                        worksheet1.Row(k).Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheet1.Row(k).Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheet1.Row(k).Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }
                    worksheet1.Cells[worksheet1.Dimension.Address].AutoFitColumns();
                    //Row1
                    worksheet1.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet1.Row(1).Height = 62.25;
                    worksheet1.Row(1).Style.Font.Bold = true;
                    worksheet1.Row(1).Style.Font.Size = 14;
                    worksheet1.Row(1).Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.None;
                    worksheet1.Row(1).Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.None;
                    worksheet1.Row(1).Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.None;
                    //Row2
                    worksheet1.Cells["B2:R2"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet1.Cells["B2:R2"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet1.Cells["B2:R2"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet1.Cells["B2:R2"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet1.Row(2).Style.Font.Size = 8;
                    worksheet1.Row(2).Style.Font.Bold = true;
                    worksheet1.Row(2).Height = 63;
                    worksheet1.Row(2).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet1.Row(2).Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    worksheet1.Row(2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet1.Row(2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    worksheet1.Column(1).Width = 22.14;
                    worksheet1.Column(2).Width = 18;
                    worksheet1.Column(3).Width = 23.43;
                    worksheet1.Column(4).Width = 25.71;
                    worksheet1.Column(6).Width = 8.43;
                    worksheet1.Column(7).Width = 8.43;
                    worksheet1.Column(9).Width = 10.71;
                    worksheet1.Column(10).Width = 11;
                    worksheet1.Column(11).Width = 8.43;
                    worksheet1.Column(12).Width = 8.43;
                    worksheet1.Column(13).Width = 8.43;
                    worksheet1.Column(14).Width = 8.43;
                    worksheet1.Column(15).Width = 8.43;
                    worksheet1.Column(16).Width = 8.43;
                    worksheet1.Column(17).Width = 8.43;
                    worksheet1.Column(18).Width = 8.43;
                    worksheet1.Column(6).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    var root = System.IO.Path.GetDirectoryName(pricePath);
                    string subdir = @$"{root}\Reports";
                    Directory.CreateDirectory(subdir);
                    System.IO.Path.Combine(root, "Reports");

                    FileInfo excelFile = new FileInfo($@"{root}\Reports\{fileName}.xlsx");
                    excel.SaveAs(excelFile);                    
                    long totalMemory = GC.GetTotalMemory(false);

                    GC.Collect(1, GCCollectionMode.Forced);
                    GC.WaitForPendingFinalizers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + "CreateExcelReport Error");
            }
        }

        public void ReadDotationsFromExcel()
        {
            try
            {
                List<Drug> listDrugs = new List<Drug>();
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                //create a list to hold all the values
                List<string> excelData = new List<string>();

                //read the Excel file as byte array
                byte[] bin = System.IO.File.ReadAllBytes(allowancePath);

                //or if you use asp.net, get the relative path
                //byte[] bin = File.ReadAllBytes(Server.MapPath("ExcelDemo.xlsx"));

                //create a new Excel package in a memorystream
                using (MemoryStream stream = new MemoryStream(bin))

                using (ExcelPackage excelPackage = new ExcelPackage(stream))
                {
                    //loop all worksheets
                    foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                    {
                        //loop all rows
                        for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                        {
                            //loop all columns in a row
                            for (int j = worksheet.Dimension.Start.Column; j <= worksheet.Dimension.End.Column; j++)
                            {
                                if (worksheet.Cells[i, j].Value != null)
                                {
                                    foreach (var r in Regions)
                                    {
                                        if (worksheet.Cells[i, j].Value.ToString().Trim() == r.Trim())
                                        {
                                            Allowance allowance = new Allowance();
                                            //allowance.Oblast.Trim();
                                            allowance.Oblast = worksheet.Cells[i, j].Value.ToString().TrimStart();
                                            if (worksheet.Cells[i + 1, j].Value.ToString() == "1 зона")
                                            {
                                                if (worksheet.Cells[i + 2, j + 1].Value.ToString().Contains("*"))
                                                {
                                                    worksheet.Cells[i + 2, j + 1].Value = worksheet.Cells[i + 2, j + 1].Value.ToString().Remove(0, 3);
                                                    worksheet.Cells[i + 2, j + 1].Value = worksheet.Cells[i + 2, j + 1].Value.ToString().Remove(worksheet.Cells[i + 2, j + 1].Value.ToString().Length - 1, 1);
                                                    worksheet.Cells[i + 3, j + 1].Value = worksheet.Cells[i + 3, j + 1].Value.ToString().Remove(0, 3);
                                                    worksheet.Cells[i + 3, j + 1].Value = worksheet.Cells[i + 3, j + 1].Value.ToString().Remove(worksheet.Cells[i + 3, j + 1].Value.ToString().Length - 1, 1);
                                                    worksheet.Cells[i + 4, j + 1].Value = worksheet.Cells[i + 4, j + 1].Value.ToString().Remove(0, 3);
                                                    worksheet.Cells[i + 4, j + 1].Value = worksheet.Cells[i + 4, j + 1].Value.ToString().Remove(worksheet.Cells[i + 4, j + 1].Value.ToString().Length - 1, 1);


                                                }
                                                allowance.Till50Whole = (worksheet.Cells[i + 2, j + 1].Value != null) ? Double.Parse(worksheet.Cells[i + 2, j + 1].Value.ToString()) : 0;
                                                allowance.Till50Retail = (worksheet.Cells[i + 2, j + 3].Value != null) ? Double.Parse(worksheet.Cells[i + 2, j + 3].Value.ToString()) : 0;
                                                allowance.Till500Whole = (worksheet.Cells[i + 3, j + 1].Value != null) ? Double.Parse(worksheet.Cells[i + 3, j + 1].Value.ToString()) : 0;
                                                allowance.Till500Retail = (worksheet.Cells[i + 3, j + 3].Value != null) ? Double.Parse(worksheet.Cells[i + 3, j + 3].Value.ToString()) : 0;
                                                allowance.After500Whole = (worksheet.Cells[i + 4, j + 1].Value != null) ? Double.Parse(worksheet.Cells[i + 4, j + 1].Value.ToString()) : 0;
                                                allowance.After500Retail = (worksheet.Cells[i + 4, j + 3].Value != null) ? Double.Parse(worksheet.Cells[i + 4, j + 3].Value.ToString()) : 0;
                                            }
                                            else
                                            {
                                                allowance.Till50Whole = (worksheet.Cells[i + 1, j + 1].Value != null) ? Double.Parse(worksheet.Cells[i + 1, j + 1].Value.ToString()) : 0;
                                                allowance.Till50Retail = (worksheet.Cells[i + 1, j + 3].Value != null) ? Double.Parse(worksheet.Cells[i + 1, j + 3].Value.ToString()) : 0;
                                                allowance.Till500Whole = (worksheet.Cells[i + 2, j + 1].Value != null) ? Double.Parse(worksheet.Cells[i + 2, j + 1].Value.ToString()) : 0;
                                                allowance.Till500Retail = (worksheet.Cells[i + 2, j + 3].Value != null) ? Double.Parse(worksheet.Cells[i + 2, j + 3].Value.ToString()) : 0;
                                                allowance.After500Whole = (worksheet.Cells[i + 3, j + 1].Value != null) ? Double.Parse(worksheet.Cells[i + 3, j + 1].Value.ToString()) : 0;
                                                allowance.After500Retail = (worksheet.Cells[i + 3, j + 3].Value != null) ? Double.Parse(worksheet.Cells[i + 3, j + 3].Value.ToString()) : 0;
                                            }
                                            ListAllowance.Add(allowance);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public void CreateSortedRegionLists(List<Drug> list)
        {
            VladimirskayaObl = new List<Drug>(list);
            Bashkiriya = new List<Drug>(list);
            IvanovskayaObl = new List<Drug>(list);
            IrkutskayaObl = new List<Drug>(list);
            KaliningradskayaObl = new List<Drug>(list);
            Krasnodarskii = new List<Drug>(list);
            Krasnoyarskii = new List<Drug>(list);
            Moskva = new List<Drug>(list);
            MO = new List<Drug>(list);
            NijnegorodskayaObl = new List<Drug>(list);
            NovgorodskayaObl = new List<Drug>(list);
            OmskayaObl = new List<Drug>(list);
            OrlovskayaObl = new List<Drug>(list);
            Primorskii = new List<Drug>(list);
            RostovskayaObl = new List<Drug>(list);
            Saha = new List<Drug>(list);
            Sverdlovskaya = new List<Drug>(list);
            tverskaya = new List<Drug>(list);
            Hakasiya = new List<Drug>(list);
        }

        public List<Drug> DeserializeExcel(List<string> list)
        {
            List<Drug> DeserializedDrugList = new List<Drug>();
            try
            {
                header = list.First().ToString();

                if (list != null)
                {
                    for (int i = 33; i < list.Count - 10; i += 11)
                    {
                        var drug = new Drug(list[i + 1], list[i], list[i + 2], list[i + 3], list[i + 4], list[i + 5], list[i + 7], list[i + 8], list[i + 9], list[i + 10], list[i + 6]);
                        DeserializedDrugList.Add(drug);
                    }
                }
                DeserializedDrugList = DeserializedDrugList.OrderBy(o => o.Name).ToList();

                return DeserializedDrugList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + "Deserialization Error");
            }

            return DeserializedDrugList;

        }

        public void ReadDrugsFromExcel()
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                byte[] bin = System.IO.File.ReadAllBytes(pricePath);
                //create a new Excel package in a memorystream
                using (MemoryStream stream = new MemoryStream(bin))

                using (ExcelPackage excelPackage = new ExcelPackage(stream))
                {
                    //ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
                    //var item = excelPackage.Workbook.Worksheets.ToList().SingleOrDefault(x => x.Name == "ОПР");
                    //excelPackage.Workbook.Worksheets.ToList().Remove(item);                   

                    //loop all worksheets
                    foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                    {
                        if (worksheet.Name == "Действующие")
                        {
                            //loop all rows
                            for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                            {
                                //loop all columns in a row
                                for (int j = worksheet.Dimension.Start.Column; j <= 11; j++)
                                {
                                    //add the cell data to the List
                                    if (worksheet.Cells[i, j].Value == null)
                                        excelData.Add(null);
                                    else
                                        excelData.Add(char.ToUpper(worksheet.Cells[i, j].Value.ToString()[0]) + worksheet.Cells[i, j].Value.ToString().Substring(1));
                                }
                            }
                        }
                        else if (worksheet.Name == "Исключенные")
                        {
                            for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                            {
                                //loop all columns in a row
                                for (int j = worksheet.Dimension.Start.Column; j <= 11; j++)
                                {
                                    //add the cell data to the List
                                    if (worksheet.Cells[i, j].Value == null)
                                        excelDataExept.Add(null);
                                    else
                                        excelDataExept.Add(char.ToUpper(worksheet.Cells[i, j].Value.ToString()[0]) + worksheet.Cells[i, j].Value.ToString().Substring(1));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "" + "Parsing Drugs Error");
            }
            finally
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    label.Background = new SolidColorBrush(Colors.White);
                    //label.Content = "Drag and Drop a Files";

                }));
            }
        }

        public void ClearAllLists()
        {
            ListAllowance.Clear();
            excelData.Clear();
            excelDataExept.Clear();
            exceptionsDrugList.Clear();
            Regions.Clear();
        }

        public void CalculateDotation()
        {
            try
            {
                foreach (var a in ListAllowance)
                {
                    if (a.Oblast == "Владимирская область")
                    {
                        foreach (var d in VladimirskayaObl)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }

                        CreateExcelReport(VladimirskayaObl, a.Oblast);
                    }
                    else if (a.Oblast == "Республика Башкортостан")
                    {
                        foreach (var d in Bashkiriya)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(Bashkiriya, a.Oblast);
                    }
                    else if (a.Oblast == "Ивановская область")
                    {
                        foreach (var d in IvanovskayaObl)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(IvanovskayaObl, a.Oblast);
                    }
                    else if (a.Oblast == "Иркутская область")
                    {
                        foreach (var d in IrkutskayaObl)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(IrkutskayaObl, a.Oblast);

                    }
                    else if (a.Oblast == "Калининградская область")
                    {
                        foreach (var d in KaliningradskayaObl)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(KaliningradskayaObl, a.Oblast);

                    }
                    else if (a.Oblast == "Краснодарский край")
                    {
                        foreach (var d in Krasnodarskii)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(Krasnodarskii, a.Oblast);
                    }
                    else if (a.Oblast == "Красноярский край")
                    {
                        foreach (var d in Krasnoyarskii)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(Krasnoyarskii, a.Oblast);
                    }
                    else if (a.Oblast == "г.Москва")
                    {
                        foreach (var d in Moskva)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(Moskva, a.Oblast);
                    }
                    else if (a.Oblast == "Московская область")
                    {
                        foreach (var d in MO)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(MO, a.Oblast);
                    }
                    else if (a.Oblast == "Нижегородская область")
                    {
                        foreach (var d in NijnegorodskayaObl)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(NijnegorodskayaObl, a.Oblast);
                    }
                    else if (a.Oblast == "Новгородская область")
                    {
                        foreach (var d in NovgorodskayaObl)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(NovgorodskayaObl, a.Oblast);
                    }
                    else if (a.Oblast == "Омская область")
                    {
                        foreach (var d in OmskayaObl)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(OmskayaObl, a.Oblast);
                    }
                    else if (a.Oblast == "Орловская область")
                    {
                        foreach (var d in OrlovskayaObl)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(OrlovskayaObl, a.Oblast);
                    }
                    else if (a.Oblast == "Приморский край")
                    {
                        foreach (var d in Primorskii)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(Primorskii, a.Oblast);
                    }
                    else if (a.Oblast == "Ростовская область")
                    {
                        foreach (var d in RostovskayaObl)
                        {
                            {
                                d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                                if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                    d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                                else if (d.PriceNoNds <= 500)
                                    d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                                else
                                    d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                                d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                                d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                                d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                                if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                    d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                                else if (d.PriceNoNds < 500)
                                    d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                                else
                                    d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                                d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                                d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                                d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                            }
                        }
                        CreateExcelReport(RostovskayaObl, a.Oblast);
                    }
                    else if (a.Oblast == "Республика Саха (Якутия)")
                    {
                        foreach (var d in Saha)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(Saha, a.Oblast);
                    }
                    else if (a.Oblast == "Свердловская область")
                    {
                        foreach (var d in Sverdlovskaya)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(Sverdlovskaya, a.Oblast);
                    }
                    else if (a.Oblast == "Тверская область")
                    {
                        foreach (var d in tverskaya)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(tverskaya, a.Oblast);
                    }
                    else if (a.Oblast == "Республика Хакасия")
                    {
                        foreach (var d in Hakasiya)
                        {
                            d.PriceWithNds = Math.Round(((d.PriceNoNds / 100) * 10) + d.PriceNoNds, 2);
                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*17%;ЕСЛИ(K2<=500;K2*14%;K2*8,5%))
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till50Whole;
                            else if (d.PriceNoNds <= 500)
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.Till500Whole;
                            else
                                d.WholesaleAllowance = (d.PriceNoNds / 100) * a.After500Whole;
                            d.WholesaleAllowance = Math.Round(d.WholesaleAllowance, 2);

                            d.WholesalePriceNoNds = Math.Round(d.PriceNoNds + d.WholesaleAllowance, 2); //оптовая без ндс
                            d.WholesalePriceWithNds = Math.Round((((d.PriceNoNds + d.WholesaleAllowance) / 100) * 10) + (d.PriceNoNds + d.WholesaleAllowance), 2); // (K2+M2)+((K2+M2)*10%)

                            if (d.PriceNoNds < 51) //ЕСЛИ(K2<51;K2*31%;ЕСЛИ(K2<=500;K2*25%;K2*19%))
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till50Retail;
                            else if (d.PriceNoNds < 500)
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.Till500Retail;
                            else
                                d.RetailAllowance = (d.PriceNoNds / 100) * a.After500Retail;
                            d.RetailAllowance = Math.Round(d.RetailAllowance, 2);
                            d.RetailPriceNoNds = Math.Round(d.WholesalePriceNoNds + d.RetailAllowance, 2);
                            d.RetailPriceWithNds = Math.Round((d.WholesalePriceNoNds + d.RetailAllowance) + (((d.WholesalePriceNoNds + d.RetailAllowance) / 100) * 10), 2);
                        }
                        CreateExcelReport(Hakasiya, a.Oblast);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }        
    }
}
