using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using System.Data.OleDb;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Excel;

namespace ParallelCoords
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Developer: Addanki Adithya
    /// </summary>

    // 
    public struct attrLevel
    {
        public int level;
        public String attrName;
    }

    public partial class MainWindow : Window
    {
        private string delimeter = ",";
        private bool hdrinrow1 = true;
        private DataTable t= null;
        private Dictionary<String, SortedSet<String>> attrDisVal = new Dictionary<string, SortedSet<String>>();
        private Dictionary<String, int> ptCoord = new Dictionary<string, int>();
        ScrollBar sli = new ScrollBar();
        Label header1 = new Label();
        String fname = "";
        int decisionSel ;
        LinkedList<String> selattrs= new LinkedList<string>();
        static int nmtimes = 0;
        String brushedAV = "";
        int brushedRowCount = 0;
        int globMinAttr = 0;
        int globMaxAttr = 6;
        String globdecAttr;
        EnumerableRowCollection<DataRow> selr;
        // Axes for Parallel Coordinates
        public MainWindow()
        {
            InitializeComponent();
        }

        // Display Information about the Application
        private void aboutClick(object sender, RoutedEventArgs e)
        {
            Window w = new Window();
            Panel p = new StackPanel();
            Label name = new Label();
            name.HorizontalAlignment = HorizontalAlignment.Center;
            name.VerticalAlignment = VerticalAlignment.Center;
            name.Content = "Parallel Coordinate Chart v1.0\n"
                +"Developer © : Adithya Addanki\nEmail: aa207@zips.uakron.edu";
            p.Children.Add(name);
            p.VerticalAlignment = VerticalAlignment.Center;
            w.Content = p;
            w.ResizeMode = ResizeMode.NoResize;
            w.Height = 100;
            w.Width = 500;
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.Name = "About";
            w.Title = "About: Parallel Coordinates";
            w.PreviewKeyDown += (s, se) =>
            {
                if (se.Key == Key.Escape)
                    w.Close();
            };
            /*Image ic=new Image();
            ic.Source=new BitmapImage(new Uri(System.AppDomain.CurrentDomain.BaseDirectory+
                                    "..\\..\\images.ico", UriKind.Relative));
            w.Icon = ic.Source;*/
            w.Focus();
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.ShowDialog();
        }

        private void clearBrush(object sender, KeyEventArgs se)
        {
            if (se.Key == Key.Escape)
            {
                brushedAV = "";
                selr = null;
                brushedRowCount = 0;
                userSel.Children.Clear();
            }
        }

        // Preferences for the Import: set delimeter and header content
        // Future incorporation into the application
        private void setPreferences(object sender, RoutedEventArgs e)
        {
            Window w = new Window();
            Panel p = new WrapPanel();
            Label name = new Label();
            name.Content = "Headers in first row?";
            CheckBox cb = new CheckBox();
            cb.VerticalAlignment = VerticalAlignment.Center;
            cb.Name = "hFirst";
            cb.IsEnabled = false;
            cb.IsChecked = true;
            cb.Checked += (s, se) =>
                            {
                                CheckBox x = s as CheckBox;
                                hdrinrow1 = x.IsChecked ?? true;
                            };
            cb.Unchecked += (s, se) =>
                            {
                                CheckBox x = s as CheckBox;
                                hdrinrow1 = x.IsChecked ?? false;
                            };
            Label delim = new Label();
            delim.Content = "Delimeter for columns?";
            ComboBox bx = new ComboBox();
            bx.VerticalAlignment = VerticalAlignment.Center;
            bx.Name = "delimSel";
            //bx.IsEnabled = false;
            ComboBoxItem comma = new ComboBoxItem();
            comma.Content = ",";
            ComboBoxItem semicol = new ComboBoxItem();
            semicol.Content = ";";
            ComboBoxItem col = new ComboBoxItem();
            col.Content = ":";
            bx.Items.Add(comma);
            bx.Items.Add(semicol);
            bx.Items.Add(col);
            RegisterName("delimSel",bx);
            Button ok = new Button();
            String sel = "";
            bx.SelectionChanged += (s, se) =>
                                    {
                                        ComboBox x = s as ComboBox;
                                        sel=x.SelectedValue.ToString();
                                        delimeter=sel.Substring(sel.Length-2);
                                    };
            ok.Content = "Apply";
            ok.Click += ok_Click;
            p.Children.Add(name);
            p.Children.Add(cb);            
            p.Children.Add(delim);
            p.Children.Add(bx);
            p.Children.Add(ok);
            p.VerticalAlignment = VerticalAlignment.Center;
            p.HorizontalAlignment = HorizontalAlignment.Center;
            w.Content = p;
            w.Height = 150;
            w.Width = 250;
            w.ResizeMode = ResizeMode.NoResize;
            w.Title = "Preferences";
            w.PreviewKeyDown += (s, se) =>
                                {
                                    if (se.Key == Key.Escape)
                                        w.Close();
                                };
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            /*Image ic = new Image();
            ic.Source = new BitmapImage(new Uri(System.AppDomain.CurrentDomain.BaseDirectory 
                            + "..\\..\\images.ico", UriKind.Relative));
            w.Icon = ic.Source;*/
            w.Focus();
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.ShowDialog();
        }

        void cb_Checked(object sender, RoutedEventArgs e)
        {
        
        }

        void ok_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cb = (ComboBox)FindName("delimSel");
            MessageBox.Show(cb.SelectionBoxItem.ToString());
        }

        // Display help information regarding how to use the application
        private void helpPC(object sender, RoutedEventArgs e)
        {
            Window w = new Window();
            Panel p = new StackPanel();
            Label name = new Label();
            name.HorizontalAlignment = HorizontalAlignment.Center;
            name.VerticalAlignment = VerticalAlignment.Center;
            name.Content = "Parallel Coordinate Chart v1.0\n"
                + "1. Load a file using 'File' menu\n"
                + "\t XLS, XLSX, CSV with headers in the first row\n"
                + "\t A preview of the data being loaded is displayed\n"
                + "2. A plot is drawn on the top frame\n"
                + "\t If the #attributes is greater than 6\n"
                + "\t\t Provision for user to select a decision variable\n"
                + "\t\t Slider to move through the chart, with decision variable fixed\n"
                + "3. The user can filter the plotted graph\n"
                + "\t Click on an axis of interest, the brushed graph is drawn frame 2\n"
                + "4. The lower frame is more customizable\n"
                + "\t Allows upto 6 attributes[Attributes of interest]\n"
                + "\t Weka Logs could be loaded[Selects top 6 attributes]\n"
                +"\n\n**Preferences have not been incorporated yet.";
                
            p.Children.Add(name);
            p.VerticalAlignment = VerticalAlignment.Center;
            w.Content = p;
            w.ResizeMode = ResizeMode.NoResize;
            w.Height = 300;
            w.Width = 500;
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.Name = "Help";
            w.Title = "Help: Parallel Coordinates";
            w.PreviewKeyDown += (s, se) =>
                            {
                                if (se.Key == Key.Escape)
                                     w.Close();
                            };
            w.BorderBrush=new SolidColorBrush(Colors.Transparent);
            /*Image ic = new Image();
            ic.Source = new BitmapImage(new Uri(System.AppDomain.CurrentDomain.BaseDirectory +
                                    "..\\..\\images.ico", UriKind.Relative));
            w.Icon = ic.Source;*/
            w.Focus();
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen; 
            w.ShowDialog();
        }
        
        // Imports CSV/XLS/XLSX using helper method
        private void loadFile(object sender, RoutedEventArgs e)
        {
            try
            {
                attrDisVal = new Dictionary<string, SortedSet<String>>();
                ptCoord = new Dictionary<string, int>();
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "DataSet(CSV or XLS[X])|*.csv;*.xls;*.xlsx";
                if (ofd.ShowDialog() == true)
                {
                    fname = ofd.FileName;
                    if (fname.Substring(fname.Length - 4).Equals(".xls"))
                    {
                        FileStream fs = File.Open(fname, FileMode.Open, FileAccess.Read);
                        IExcelDataReader er = Excel.ExcelReaderFactory.CreateBinaryReader(fs);
                        t = loadData(er);
                        DataGrid data = new DataGrid();
                        data.ItemsSource = t.DefaultView;
                        Window w = new Window();
                        Panel temp = new WrapPanel();
                        temp.Children.Add(data);
                        w.Content = temp;
                        w.Title = "Preview";
                        w.Closed += w_Closed;
                        w.Closing += w_Closing;
                        w.ResizeMode = ResizeMode.NoResize;
                        /*Image ic = new Image();
                        ic.Source = new BitmapImage(new Uri(System.AppDomain.CurrentDomain.BaseDirectory
                                    + "..\\..\\images.ico", UriKind.Relative));
                        w.Icon = ic.Source;*/
                        w.Focus();
                        w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        w.PreviewKeyDown += (s, se) =>
                        {
                            if (se.Key == Key.Escape)
                                w.Close();
                        };
                        w.ShowDialog();
                        er.Close();
                        fs.Close();                        
                    }
                    else if (fname.Substring(fname.Length - 5).Equals(".xlsx"))
                    {
                        FileStream fs = File.Open(fname, FileMode.Open, FileAccess.Read);
                        IExcelDataReader er = Excel.ExcelReaderFactory.CreateOpenXmlReader(fs);
                        t = loadData(er);
                        DataGrid data = new DataGrid();
                        data.ItemsSource = t.DefaultView;
                        Window w = new Window();
                        Panel temp = new WrapPanel();
                        temp.Children.Add(data);
                        w.Content = temp;
                        w.Title = "Preview";
                        w.ResizeMode = ResizeMode.NoResize;
                        w.Closing += w_Closing;
                        w.Closed += w_Closed;
                        /*Image ic = new Image();
                        ic.Source = new BitmapImage(new Uri(System.AppDomain.CurrentDomain.BaseDirectory
                                    + "..\\..\\images.ico", UriKind.Relative));
                        w.Icon = ic.Source;*/
                        w.Focus();
                        w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        w.PreviewKeyDown += (s, se) =>
                        {
                            if (se.Key == Key.Escape)
                                w.Close();
                        };
                        w.ShowDialog();
                        er.Close();
                        fs.Close();                       
                    }
                    else if (fname.Substring(fname.Length - 4).Equals(".csv"))
                    {
                        FileStream fs = File.Open(fname, FileMode.Open, FileAccess.Read);
                        StreamReader sr = new StreamReader(fs);
                        t = loadCSV(sr);
                        DataGrid data = new DataGrid();
                        data.ItemsSource = t.DefaultView;
                        Window w = new Window();
                        Panel temp = new WrapPanel();
                        temp.Children.Add(data);
                        w.Content = temp;
                        w.Title = "Preview";
                        w.Closed += w_Closed;
                        w.Closing += w_Closing;
                        w.ResizeMode = ResizeMode.NoResize;
                        /*Image ic = new Image();
                        ic.Source = new BitmapImage(new Uri(System.AppDomain.CurrentDomain.BaseDirectory
                                    + "..\\..\\images.ico", UriKind.Relative));
                        w.Icon = ic.Source;*/
                        w.Focus();
                        w.PreviewKeyDown += (s, se) =>
                        {
                            if (se.Key == Key.Escape)
                                w.Close();
                        };
                        w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        w.ShowDialog();
                        sr.Close();
                        fs.Close();                        
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Issue reading file");
            }
        }

        // loads CSV file with provided delimeter; default delimeter [,]
        private DataTable loadCSV(StreamReader er)
        {
            DataTable t = new DataTable();            
            DataColumn[] cols;
            int rownum = 0;
            String line;
            
            while ((line=er.ReadLine())!=null)
            {
                int i = 0;
                String[] tokens = line.Split(delimeter.ToCharArray());
                if (rownum == 0)
                {
                    cols = new DataColumn[tokens.Length];
                    while (i < tokens.Length)
                    {
                        cols[i] = new DataColumn(tokens[i].TrimEnd().TrimStart());
                        cols[i].DataType = System.Type.GetType("System.String");
                        t.Columns.Add(cols[i]);
                        i++;
                    }
                    rownum++;
                }
                else if (rownum != 0)
                {
                    DataRow row = t.NewRow();
                    while (i < tokens.Length)
                    {
                        row[t.Columns[i]] = tokens[i];
                        i++;
                    }
                    t.Rows.Add(row);
                }                   
            }            
            er.Close();            
            return t;
        }

        // helper method to load from Excel
        private DataTable loadData(IExcelDataReader er)
        {            
            er.IsFirstRowAsColumnNames = true;
            DataSet ds = er.AsDataSet();
            DataTable t = new DataTable();
            DataColumn[] cols;
            int rownum = 0;
            while (er.Read())
            {
                int i = 0;
                if (rownum == 0)
                {
                    cols = new DataColumn[er.FieldCount];
                    while (i < er.FieldCount)
                    {
                        cols[i] = new DataColumn(er.GetString(i).TrimEnd().TrimStart());
                        cols[i].DataType = System.Type.GetType("System.String");
                        t.Columns.Add(cols[i]);
                        i++;
                    }
                    rownum++;
                }
                else if (rownum != 0)
                {
                    DataRow row = t.NewRow();
                    while (i < er.FieldCount)
                    {
                        row[t.Columns[i]] = er.GetString(i);
                        i++;
                    }
                    t.Rows.Add(row);
                }                
            }
            er.Close();
            return t;
        }

        // Confirmation to close the application
        private void closingCheck(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult mb = MessageBox.Show("Really want to quit?","Sure?",
                                        MessageBoxButton.YesNo,MessageBoxImage.Warning);
            if (mb == MessageBoxResult.No)
                e.Cancel = true;
        }

        private void closedWindow(object sender, EventArgs e)
        {

        }

        void w_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            invisiAxes(0);
        }
        
        // Once the preview closes, loads the datagrid into the parallel coordinates
        void w_Closed(object sender, EventArgs e)
        {
            try
            {
                invisiAxes(0);
                nmtimes++;
                if (nmtimes > 1)
                {
                    ComboBox cb1 = (ComboBox)attrDDC.FindName("Preview");
                    ComboBox cb2 = (ComboBox)attrDDC.FindName("select");
                    cb1.Items.Clear();
                    cb2.Items.Clear();
                    UnregisterName("select");
                    UnregisterName("Preview");
                    UnregisterName("Weka");
                    UnregisterName("DrawPlot");
                }
                selattrs.Clear();
                attrDisVal.Clear();
                ptCoord.Clear();
                pcoord.Children.Clear();
                decAttr.Children.Clear();
                userSel.Children.Clear();
                attrDDC.Children.Clear();
                brushedAV = "";
                selr = null;
                userSel.Children.Clear();
                brushedRowCount = 0;
                globMinAttr = 0;
                globMaxAttr = 6;
                globdecAttr = "";
                int i = 0;
                int cols = t.Columns.Count;
                sli = new ScrollBar();
                sli.Name = "slider";
                sli.Margin = new Thickness(18, 320, 0, 0);
                sli.Orientation = Orientation.Horizontal;
                sli.SmallChange = 1;
                sli.Scroll += slider_ValueChanged;
                sli.Height = 15;
                sli.Width = 700;
                sli.Visibility = (cols < 6 ? Visibility.Hidden : Visibility.Visible);
                sli.Minimum = 0;
                sli.Maximum = (cols / 6) - 1;
                int rem = cols % 6;
                if (rem > 0)
                    sli.Maximum += 1;

                checkDecisionAttr();
                initTable();
                initPlotPoints();

                pcoord.Children.Add(sli);
                globMinAttr = 0;
                globMaxAttr = (cols < 6 ? cols : 6);
                drawPlot(globMinAttr, globMaxAttr);
                while (i < (cols < 6 ? cols : 6))
                {
                    drawAxes(i, i);
                    i++;
                }
                if (cols > 6)
                {
                    drawAxes(i, decisionSel);
                    String temp = t.Columns[decisionSel].ColumnName; ;
                    Label Attr1 = new Label();
                    Attr1.Margin = new Thickness(850, 275, 0, 0);
                    Attr1.Visibility = Visibility.Visible;
                    Attr1.Content = temp;
                    Attr1.ToolTip = getDisVals(attrDisVal[temp]);
                    pcoord.Children.Add(Attr1);
                }
                header1 = new Label();
                header1.Margin = new Thickness(100, 295, 0, 0);
                header1.Content = "Parallel Coordinates : " + fname;
                header1.FontFamily = new FontFamily("Times New Roman");
                header1.FontWeight = FontWeights.Bold;
                header1.FontSize = 15;
                header1.Foreground = new SolidColorBrush(Colors.Blue);
                pcoord.Children.Add(header1);
                loadSecondCanvas();
            }
            catch(Exception)
            {
                MessageBox.Show("Error Reading File\nPlease check the delimeter settings in 'Preferences'");
                userSel.Children.Clear();
                attrDDC.Children.Clear();
                pcoord.Children.Clear();
                decAttr.Children.Clear();
            }
        }


        // A separate frame for user selected atributes and weka dominant attributes
        private void loadSecondCanvas()
        {
            Label ao = new Label();
            ao.Content = "Attributes";
            ao.FontFamily = new FontFamily("Times New Roman");
            ao.FontWeight = FontWeights.Bold;
            ao.FontSize = 15;
            ao.Foreground = new SolidColorBrush(Colors.Blue);
            ao.Margin = new Thickness(10, 10, 0, 0);
            attrDDC.Children.Add(ao);
            ComboBox atts = new ComboBox();
            atts.Width =85;
            atts.Name = "select";
            atts.Items.Clear();
            //atts.SelectionMode = SelectionMode.Extended;
            foreach (DataColumn dc in t.Columns)
            {
                ComboBoxItem li = new ComboBoxItem();
                li.Content = dc.ColumnName.TrimEnd().TrimStart();
                li.Name = dc.ColumnName.TrimEnd().TrimStart();
                atts.Items.Add(li);
            }
            atts.Margin = new Thickness(12,40,0,0);
            RegisterName("select", atts);
            attrDDC.Children.Add(atts);
            Button addB = new Button();
            addB.Content = "Add";
            addB.Width = 85;
            addB.Name = "addB";
            addB.Margin = new Thickness(12,70,0,0);
            addB.Click += addB_Click;           
            Label selec = new Label();
            selec.Content = "Selected";
            selec.FontFamily = new FontFamily("Times New Roman");
            selec.FontWeight = FontWeights.Bold;
            selec.FontSize = 15;
            selec.Foreground = new SolidColorBrush(Colors.Blue);
            selec.Margin = new Thickness(10, 110, 0, 0);
            attrDDC.Children.Add(selec);
            Button reset = new Button();
            reset.Content = "Clear";
            reset.Margin = new Thickness(100,140,0,0);
            reset.Height = 50; 
            reset.Click += reset_Click;
            ComboBox attr = new ComboBox();
            attr.Width = 85;
            attr.Name = "Preview";
            attr.Items.Clear();
            //atts.SelectionMode = SelectionMode.Extended;
            foreach (String dc in selattrs)
            {
                ComboBoxItem li = new ComboBoxItem();
                li.Content = dc;
                li.Name = dc;
                attr.Items.Add(li);
            }
            attr.Margin = new Thickness(12, 140, 0, 0);
            RegisterName("Preview", attr);
            Button remB = new Button();
            remB.Content = "Remove";
            remB.Name = "remB";
            remB.Width = 85;
            remB.Margin = new Thickness(12, 170, 0, 0);
            remB.Click += remB_Click;
            attrDDC.Children.Add(reset);
            attrDDC.Children.Add(addB);
            attrDDC.Children.Add(remB);
            attrDDC.Children.Add(attr);
            Button dr = new Button();
            dr.Content = "Draw Plot";
            dr.Margin = new Thickness(12,220,0,0);
            dr.Name = "DrawPlot";
            RegisterName("DrawPlot",dr);
            dr.Width = 120;
            dr.Click += dr_Click;
            attrDDC.Children.Add(dr);
            Button Weka = new Button();
            Weka.Content = "Weka Clusters";
            Weka.Margin = new Thickness(12, 250, 0, 0);
            Weka.Name = "Weka";
            Weka.Width = 120;
            Weka.Height = 30;
            RegisterName("Weka",Weka);
            Weka.Background = new SolidColorBrush(Colors.LightSkyBlue);
            Weka.Click += Weka_Click;
            attrDDC.Children.Add(Weka);
        }

        // Clears user selection
        void reset_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cb1 = (ComboBox)attrDDC.FindName("Preview");
            ComboBox cb = (ComboBox)attrDDC.FindName("select");
            for (int i = cb1.Items.Count-1;i>=0;i-- )
            {
                ComboBoxItem cbi = (ComboBoxItem)cb1.Items[i];
                selattrs.Remove(cbi.Name);
                cb1.Items.Remove(cbi);
                cb.Items.Add(cbi);
            }
            userSel.Children.Clear();
        }

        // File dialog for Weka Log file load
        void Weka_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Weka Logs(*.log,*.txt)|*.log;*.txt";
            LinkedList<attrLevel> wList = new LinkedList<attrLevel>();
            SortedSet<String> level1 = new SortedSet<string>();
            SortedSet<String> level2 = new SortedSet<string>();
            SortedSet<String> level3 = new SortedSet<string>();
            SortedSet<String> level4 = new SortedSet<string>();
            SortedSet<String> level5 = new SortedSet<string>();
            SortedSet<String> level6 = new SortedSet<string>();
            SortedSet<String> level7 = new SortedSet<string>();
            selattrs.Clear();
            if (ofd.ShowDialog() == true)
            {
                String fname = ofd.FileName;
                FileStream fs = File.Open(fname, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                String line;
                String wekaAttrs="";
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("J48 pruned tree"))
                    {
                        line = sr.ReadLine();
                        line = sr.ReadLine();
                        while(line.TrimEnd().TrimStart()=="")
                            line = sr.ReadLine();
                        while(line.IndexOf("Number of Leaves")<0)
                        {
                            wekaAttrs += line + "\t";
                            if (line.Contains("="))
                            {
                                 attrLevel al=processLine(line);
                                 if (al.level == 1)
                                     level1.Add(al.attrName);
                                 else if (al.level == 2)
                                     level2.Add(al.attrName);
                                 else if (al.level == 3)
                                     level3.Add(al.attrName);
                                 else if (al.level == 4)
                                     level4.Add(al.attrName);
                                 else if (al.level == 5)
                                     level5.Add(al.attrName);
                                 else if (al.level == 6)
                                     level6.Add(al.attrName);
                                 else if (al.level == 7)
                                     level7.Add(al.attrName);
                            }
                            line = sr.ReadLine();
                        }
                    }
                }
                sr.Close();
                fs.Close();
                MessageBox.Show("Number of Attributes(Processed from Log): \n" +
                    "Level 1:" + level1.Count + "\n" + "Level 2:" + level2.Count + "\n"
                    + "Level 3:" + level3.Count + "\n" + "Level 4:" + level4.Count + "\n"
                    + "Level 5:" + level5.Count + "\n" + "Level 6:" + level6.Count + "\n"
                    + "Level 7:" + level7.Count + "\n");
            }

            // Reset the dropdown selections 
            ComboBox cb1 = (ComboBox)attrDDC.FindName("Preview");
            cb1.Items.Clear();
            ComboBox cb = (ComboBox)attrDDC.FindName("select");
            /*for (int i = cb1.Items.Count - 1; i >= 0; i--)
            {
                ComboBoxItem cbi = (ComboBoxItem)cb1.Items[i];
                selattrs.Remove(cbi.Name);
                cb1.Items.Remove(cbi);
                cb.Items.Add(cbi);
            }*/
            cb.Items.Clear();
            //atts.SelectionMode = SelectionMode.Extended;
            foreach (DataColumn dc in t.Columns)
            {
                ComboBoxItem li = new ComboBoxItem();
                li.Content = dc.ColumnName;
                li.Name = dc.ColumnName;
                cb.Items.Add(li);
            }
            
            // Make selections from weka log
            addToDropdown(level1);
            addToDropdown(level2);
            addToDropdown(level3);
            addToDropdown(level4);
            addToDropdown(level5);
            addToDropdown(level6);
            addToDropdown(level7);
        }

        // manipulations to user selected attributes
        private void addToDropdown(SortedSet<String> level)
        {
            ComboBox cb1 = (ComboBox)attrDDC.FindName("Preview");
            ComboBox cb = (ComboBox)attrDDC.FindName("select");
            Button b = (Button)attrDDC.FindName("DrawPlot");
            foreach (string s in level)
                if (cb1.Items.Count < 6 )
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content = s.TrimEnd().TrimStart();
                    cbi.Name = s.TrimEnd().TrimStart();
                    bool flag = false;
                    ComboBoxItem temp=new ComboBoxItem();
                    foreach (ComboBoxItem c in cb.Items)
                    {
                        if (c.Content.Equals(s.TrimEnd().TrimStart()))
                        {
                            flag = true;
                            temp = c;
                            break;
                        }
                    }
                    if (flag == true)
                    {
                        cb.Items.Remove(temp);
                        selattrs.AddLast(cbi.Name);
                        cb1.Items.Add(cbi);
                        b.IsEnabled = true;
                    }
                }
        }

        // read and load the weka log, break the lines to identify the attributes
        private attrLevel processLine(String line)
        {
            attrLevel atr = new attrLevel();
            String sb = line.Substring(0,line.IndexOf("="));
            String[] temp = sb.Split(new char[]{'|'});
            atr.level=temp.Length;
            atr.attrName=temp[temp.Length-1];
            return atr;
        }

        // draw the second parallel coordinate chart based on just the attributes of interest.
        void dr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                userSel.Children.Clear();
                int c = 0;
                int i = 0, j = 0;
                string prevAttr = "";
                Label lh = new Label();
                lh.Margin = new Thickness(100, 288, 0, 0);
                lh.Content = "Parallel Coordinates : ";
                foreach (string atn in selattrs)
                {
                    if (i < selattrs.Count)
                    {
                        visiAxes(j++, c++, 0, userSel, atn);
                    }
                    Line arcPath = new Line();
                    arcPath.X1 = i * 140 + 20;
                    arcPath.Y1 = 20;
                    arcPath.X2 = i * 140 + 20;
                    arcPath.Y2 = 270;
                    arcPath.Stroke = new SolidColorBrush(Colors.Red);
                    arcPath.StrokeThickness = 7;
                    arcPath.Fill = new SolidColorBrush(Colors.Yellow);
                    arcPath.Name = "axis" + i;
                    arcPath.HorizontalAlignment = HorizontalAlignment.Center;
                    arcPath.ToolTip = "Axis|Dimension: " + atn;
                    userSel.Children.Add(arcPath);
                    if (i < selattrs.Count)
                    {
                        String col1, col2;
                        if (!prevAttr.Equals(""))
                            for (int r = 0; r < t.Rows.Count; r++)
                            {
                                String stpt = t.DefaultView[r][prevAttr].ToString();
                                col1 = prevAttr;
                                String key1 = col1 + ":" + stpt;
                                Line pline = new Line();
                                pline.X1 = (i - 1) * 140 + 20;
                                pline.Y1 = ptCoord[key1];
                                col2 = atn;
                                String endpt = t.DefaultView[r][atn].ToString();
                                String key2 = col2 + ":" + endpt;
                                pline.X2 = (i) * 140 + 20;
                                pline.Y2 = ptCoord[key2];
                                pline.Stroke = new SolidColorBrush(Colors.Black);
                                pline.StrokeThickness = 1;
                                pline.Fill = new SolidColorBrush(Colors.Blue);
                                pline.HorizontalAlignment = HorizontalAlignment.Center;
                                pline.ToolTip = key1 + "---" + key2;
                                userSel.Children.Add(pline);
                            }
                    }
                    prevAttr = atn;
                    lh.Content += "\t" + atn;
                    i++;
                }
                lh.FontFamily = new FontFamily("Times New Roman");
                lh.FontWeight = FontWeights.Bold;
                lh.FontSize = 15;
                lh.Foreground = new SolidColorBrush(Colors.Blue);
                userSel.Children.Add(lh);
            }
            catch (Exception exp)
            {
                pcoord.Children.Clear();
                userSel.Children.Clear();
                attrDDC.Children.Clear();
                decAttr.Children.Clear();
            }
        }

        // manipulations to user selected attributes-- Remove
        void remB_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cb1 = (ComboBox)attrDDC.FindName("Preview");
            ComboBox cb = (ComboBox)attrDDC.FindName("select");
            ComboBoxItem cbi = (ComboBoxItem)cb1.SelectedItem;
            if (cb1.SelectedIndex != -1)
            {
                selattrs.Remove(cbi.Name);
                cb1.Items.Remove(cbi);
                cb.Items.Add(cbi);
            }
        }

        // manipulations to user selected attributes-- Add
        void addB_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cb = (ComboBox)attrDDC.FindName("select");
            ComboBox cb1 = (ComboBox)attrDDC.FindName("Preview");
            ComboBoxItem cbi = (ComboBoxItem)cb.SelectedItem;
            if (cb.SelectedIndex != -1)
            {
                if (cb1.Items.Count < 6)
                {
                    cb.Items.Remove(cbi);
                    selattrs.AddLast(cbi.Name);
                    cb1.Items.Add(cbi);
                }
                else
                {
                    MessageBox.Show("Please add only 6 attributes at once","Caution");
                }
            }
            userSel.Children.Clear();
        }

        // loads the decision vars when the #attributes is >6
        private void checkDecisionAttr()
        {
            decAttr.Children.Clear();
            int cols = t.Columns.Count;
            if (cols > 6)
            {
                Label ao = new Label();
                ao.Content = "Analyze";
                ao.FontFamily = new FontFamily("Times New Roman");
                ao.FontWeight = FontWeights.Bold;
                ao.FontSize = 15;
                ao.Foreground = new SolidColorBrush(Colors.Blue);
                String name = t.Columns[--cols].ColumnName;
                globdecAttr = name;
                ao.Margin = new Thickness(10,10,0,0);
                decAttr.Children.Add(ao);
                RadioButton rb1 = new RadioButton();
                rb1.Content = name;
                rb1.Name = "_"+cols ;
                decisionSel = cols;
                rb1.Margin = new Thickness(15,30,0,0);
                rb1.IsChecked = true; 
                rb1.Checked += rb_Checked;                
                RadioButton rb2 = new RadioButton();
                name = t.Columns[--cols].ColumnName; 
                rb2.Content = name;
                rb2.Name = "_" + cols;
                rb2.Margin = new Thickness(15, 50, 0, 0);
                rb2.Checked += rb_Checked;
                RadioButton rb3 = new RadioButton();
                name = t.Columns[--cols].ColumnName; 
                rb3.Content = name;
                rb3.Name = "_" + cols;
                rb3.Margin = new Thickness(15, 70, 0, 0);
                rb3.Checked += rb_Checked;
                name = t.Columns[--cols].ColumnName; 
                RadioButton rb4 = new RadioButton();
                rb4.Content = name;
                rb4.Name = "_" + cols;
                rb4.Margin = new Thickness(15, 90, 0, 0);
                rb4.Checked += rb_Checked;
                name = t.Columns[--cols].ColumnName; 
                RadioButton rb5 = new RadioButton();
                rb5.Content = name;
                rb5.Name = "_" + cols; 
                rb5.Margin = new Thickness(15, 110, 0, 0);
                rb5.Checked += rb_Checked;
                decAttr.Children.Add(rb1);
                decAttr.Children.Add(rb2);
                decAttr.Children.Add(rb3);
                decAttr.Children.Add(rb4);
                decAttr.Children.Add(rb5);
            }
        }

        // redraw the plot when the decision var is changed
        void rb_Checked(object sender, RoutedEventArgs e)
        {
            userSel.Children.Clear();
            brushedAV = "";
            RadioButton rb1= sender as RadioButton;
            if (rb1.IsChecked??false )
            {
                String rn = rb1.Name;
                decisionSel = Convert.ToInt32(rn.Substring(rn.IndexOf("_")+1));
                ScrollBar slider = (ScrollBar)pcoord.FindName("slider");
                invisiAxes(0);
                int intCoordInd = 0;
                int min = intCoordInd * 6;
                int max = ((intCoordInd + 1) * 6);
                max = max > t.Columns.Count ? t.Columns.Count : max;
                int i = 0, j = min;
                
                globMinAttr = min;
                globMaxAttr = max;
                drawPlot(min, max);
                while (i < ((max / 6) > 0 ? 6 : max % 6))
                {
                    drawAxes(i, j++);
                    i++;
                }
                if (max < t.Columns.Count)
                {
                    drawAxes(i, decisionSel);
                    String temp = t.Columns[decisionSel].ColumnName; ;
                    Label Attr1 = new Label();
                    Attr1.Margin = new Thickness(850, 275, 0, 0);
                    Attr1.Visibility = Visibility.Visible;
                    Attr1.Content = temp;
                    Attr1.ToolTip = getDisVals(attrDisVal[temp]);
                    pcoord.Children.Add(Attr1);
                    globdecAttr = temp;
                }
                //MessageBox.Show(""+decisionSel);
            }
        }

        // Drawing axes for the coordinates
        private void drawAxes(int i,int j)
        {
            if(j<t.Columns.Count)
            {
                Line arcPath = new Line();
                arcPath.X1 = i * 140 + 20;
                arcPath.Y1 = 20;
                arcPath.X2 = i * 140 + 20;
                arcPath.Y2 = 270;
                arcPath.Stroke = new SolidColorBrush(Colors.Red);
                arcPath.StrokeThickness = 7;
                arcPath.Fill = new SolidColorBrush(Colors.Yellow);
                arcPath.Name = "axis" + i;
                arcPath.HorizontalAlignment = HorizontalAlignment.Center;
                arcPath.ToolTip = "Axis|Dimension: " + t.Columns[j].ColumnName;
                Nullable<Point> dragStart = null;
                Nullable<Point> dragEnd = null;
                arcPath.MouseLeftButtonDown += (sender, args) =>
                {
                    var element = (UIElement)sender;
                    dragStart = args.GetPosition(element);
                    element.CaptureMouse();
                };
                arcPath.MouseLeftButtonUp += (sender, args) =>
                {
                    var element = (UIElement)sender;
                    dragEnd = args.GetPosition(element);
                    if (dragStart != null )
                    {
                        //MessageBox.Show("Attribute:" + t.Columns[j].ColumnName+ ":" + dragStart + ":" + dragEnd);
                        brushed(t.Columns[j].ColumnName, dragStart, dragEnd);
                    }
                };                
                pcoord.Children.Add(arcPath);
            }
        }

        private void brushed(String colname, Nullable<Point> ds, Nullable<Point> de)
        {
            try
            {
                string attrVal = "";
                brushedAV = "";
                selr = null;
                brushedRowCount = 0;
                userSel.Children.Clear();
                if (de != null)
                {
                    Point s = ds ?? new Point(0, 0);
                    Point p = de ?? new Point(0, 0);
                    if (p.X != 0 && p.Y != 0 && s.X != 0 && s.Y != 0)
                    {
                        foreach (string tmp in attrDisVal[colname])
                        {
                            //MessageBox.Show(ptCoord[colname + ":" + tmp]+"");
                            if (ptCoord[colname + ":" + tmp] > (p.Y - 5) && ptCoord[colname + ":" + tmp] < (p.Y + 5))
                            {
                                attrVal = tmp;
                                break;
                            }
                        }
                        if (!attrVal.Equals(""))
                        {
                            userSel.Children.Clear();
                            brushedAV += colname + ":" + attrVal + "  |  ";
                            //MessageBox.Show(brushedAV);
                            drawBrushedPlot(brushedAV);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Something went wrong.. Please try again");
                userSel.Children.Clear();
                brushedAV = "";
                selr = null;
                brushedRowCount = 0;
            }
        }

        private void drawBrushedPlot(String bAV)
        {
            brushedRowCount = 0;
            if (!bAV.Equals(""))
            {
                string[] avPairs = bAV.Split(new char[]{'|'});
                foreach (string av in avPairs)
                {
                    if (!av.Trim().Equals(""))
                    {
                        string[] cv = av.Trim().Split(new char[]{':'});
                        if (cv.Length == 2)
                        {
                            if (selr==null)
                            {
                                var selRows = from sr in t.AsEnumerable()
                                              where sr.Field<String>(cv[0]) == cv[1]
                                              select sr;
                                selr = selRows;
                                brushedRowCount += selRows.Count();
                                drawSelected(globMinAttr, globMaxAttr, selRows);
                            }
                            else
                            {
                                var selRows = from sr in selr
                                              where sr.Field<String>(cv[0]) == cv[1]
                                              select sr;
                                selr = selRows;
                                brushedRowCount += selRows.Count();
                                drawSelected(globMinAttr, globMaxAttr, selRows);
                            }
                        }                         
                    }
                }
                //MessageBox.Show("Select number of rows: "+brushedRowCount);
            }
        }
        // Tooltip to display the distinct values on the axis label
        private String getDisVals(SortedSet<string> temp)
        {
            String tmp="[ ";
            foreach(String x in temp)
            {
                tmp += x + " ";
            }
            tmp += "]";
            return tmp;
        }

        // Updates and makes the axes visible
        private void visiAxes(int i,int j, int k, Canvas pcoord,string tp)
        {
            try
            {
                String temp = t.Columns[j].ColumnName; ;
                if (i == 0)
                {
                    Label Attr1 = new Label();
                    Attr1.Margin = new Thickness(10, 275, 0, 0);
                    Attr1.Visibility = Visibility.Visible;
                    if (tp.Equals(""))
                    {
                        Attr1.Content = temp;
                        Attr1.ToolTip = getDisVals(attrDisVal[temp]);
                    }
                    else
                    {
                        Attr1.Content = tp;
                        Attr1.ToolTip = getDisVals(attrDisVal[tp]);
                    }
                    pcoord.Children.Add(Attr1);
                }
                else if (i == 1)
                {
                    Label Attr2 = new Label();
                    Attr2.Margin = new Thickness(150, 275, 0, 0);
                    Attr2.Visibility = Visibility.Visible;
                    if (tp.Equals(""))
                    {
                        Attr2.Content = temp;
                        Attr2.ToolTip = getDisVals(attrDisVal[temp]);
                    }
                    else
                    {
                        Attr2.Content = tp;
                        Attr2.ToolTip = getDisVals(attrDisVal[tp]);
                    }
                    pcoord.Children.Add(Attr2);
                }
                else if (i == 2)
                {
                    Label Attr3 = new Label();
                    Attr3.Margin = new Thickness(290, 275, 0, 0);
                    Attr3.Visibility = Visibility.Visible;
                    if (tp.Equals(""))
                    {
                        Attr3.Content = temp;
                        Attr3.ToolTip = getDisVals(attrDisVal[temp]);
                    }
                    else
                    {
                        Attr3.Content = tp;
                        Attr3.ToolTip = getDisVals(attrDisVal[tp]);
                    }
                    pcoord.Children.Add(Attr3);
                }
                else if (i == 3)
                {
                    Label Attr4 = new Label();
                    Attr4.Margin = new Thickness(430, 275, 0, 0);
                    Attr4.Visibility = Visibility.Visible;
                    if (tp.Equals(""))
                    {
                        Attr4.Content = temp;
                        Attr4.ToolTip = getDisVals(attrDisVal[temp]);
                    }
                    else
                    {
                        Attr4.Content = tp;
                        Attr4.ToolTip = getDisVals(attrDisVal[tp]);
                    }
                    pcoord.Children.Add(Attr4);
                }
                else if (i == 4)
                {
                    Label Attr5 = new Label();
                    Attr5.Margin = new Thickness(570, 275, 0, 0);
                    Attr5.Visibility = Visibility.Visible;
                    if (tp.Equals(""))
                    {
                        Attr5.Content = temp;
                        Attr5.ToolTip = getDisVals(attrDisVal[temp]);
                    }
                    else
                    {
                        Attr5.Content = tp;
                        Attr5.ToolTip = getDisVals(attrDisVal[tp]);
                    }
                    pcoord.Children.Add(Attr5);

                }
                else if (i == 5)
                {
                    Label Attr6 = new Label();
                    Attr6.Margin = new Thickness(710, 275, 0, 0);
                    Attr6.Visibility = Visibility.Visible;
                    if (tp.Equals(""))
                    {
                        Attr6.Content = temp;
                        Attr6.ToolTip = getDisVals(attrDisVal[temp]);
                    }
                    else
                    {
                        Attr6.Content = tp;
                        Attr6.ToolTip = getDisVals(attrDisVal[tp]);
                    }
                    pcoord.Children.Add(Attr6);
                }
            }
            catch (KeyNotFoundException e)
            {
                MessageBox.Show("Please verify that the files provided are "
                                    +"correct[Attribute Mismatch]","Error");
                pcoord.Children.Clear();
                userSel.Children.Clear();
                attrDDC.Children.Clear();
                decAttr.Children.Clear();
            }
        }

        //drawAxes for Brushed Plot
        private void drawAxesBrushed(int i, int j)
        {
            if (j < t.Columns.Count)
            {
                Line arcPath = new Line();
                arcPath.X1 = i * 140 + 20;
                arcPath.Y1 = 20;
                arcPath.X2 = i * 140 + 20;
                arcPath.Y2 = 270;
                arcPath.Stroke = new SolidColorBrush(Colors.Red);
                arcPath.StrokeThickness = 4;
                arcPath.Fill = new SolidColorBrush(Colors.Yellow);
                arcPath.Name = "axis" + i;
                arcPath.HorizontalAlignment = HorizontalAlignment.Center;
                arcPath.ToolTip = "Axis|Dimension: " + t.Columns[j].ColumnName;
                userSel.Children.Add(arcPath);
            }
        }

        // Clears the canvas
        private void invisiAxes(double sliVal)
        {
            int cols = t.Columns.Count;
            pcoord.Children.Clear();
            sli = new ScrollBar();
            sli.Name = "slider";
            sli.Margin = new Thickness(18, 320, 0, 0);
            sli.Orientation = Orientation.Horizontal;
            sli.SmallChange = 1;
            sli.Scroll += slider_ValueChanged;
            sli.Height = 15;
            sli.Width = 700;
            sli.Visibility = (cols < 6 ? Visibility.Hidden : Visibility.Visible);
            sli.Minimum = 0;
            sli.Maximum = (cols / 6) - 1;
            int rem = cols % 6;
            if (rem > 0)
                sli.Maximum += 1;
            sli.Value = sliVal;
            pcoord.Children.Add(sli);
            header1 = new Label();
            header1.Margin = new Thickness(100, 295, 0, 0);
            header1.Content = "Parallel Coordinates : " + fname;
            header1.FontFamily = new FontFamily("Times New Roman");
            header1.FontWeight = FontWeights.Bold;
            header1.FontSize = 15;
            header1.Foreground= new SolidColorBrush(Colors.Blue);
            pcoord.Children.Add(header1);
        }
        
        // loads the table into distinct value sets
        private void initTable()
        {
            int c = 0, r = 0;
            while (c < t.Columns.Count)
            {
                SortedSet<String> tempset = new SortedSet<String>();
                while (r < t.Rows.Count)
                {
                    tempset.Add(t.DefaultView[r][c].ToString());
                    r++;
                }
                attrDisVal.Add(t.Columns[c].ColumnName,tempset);
                c++;
                r = 0;
            }
        }

        // Determines the plot points for the axes
        private void initPlotPoints()
        {
            int c = 0, r = 0;
            int ymax = 270;
            int ymin = 20;
            int indycord = 0;
            while (c < t.Columns.Count)
            {
                r = 0;
                int uSize=attrDisVal[t.Columns[c].ColumnName].Count;
                foreach (string tmp in attrDisVal[t.Columns[c].ColumnName])
                {
                    if (uSize > 1)
                        indycord = (((ymax - ymin) / (uSize - 1)) * r) + 20;
                    else
                        indycord = ymin;
                    ptCoord.Add(t.Columns[c].ColumnName + ":" + tmp,indycord);
                    r++;
                }
                c++;
            }
        }

        // Draws the plot on the canvas
        private void drawSelected(int attrInd, int attrmax,EnumerableRowCollection<DataRow> st)
        {
            try
            {
                if (st.Count() > 0)
                {
                    int r = 0, c = attrInd;
                    int cols = t.Columns.Count;
                    String col1 = "";
                    String col2 = "";

                    int i1 = 0, j1 = attrInd;
                    

                    for (int i = attrInd, j = 0; i < attrmax; i++)
                        visiAxes(j++, c++, 0, userSel, "");
                    foreach (DataRow dr in st)
                    {
                        c = attrInd;
                        int k = 0;
                        for (; c < (attrmax) - 1; )
                        {
                            String stpt = dr[c].ToString();
                            col1 = t.Columns[c].ColumnName;
                            String key1 = col1 + ":" + stpt;
                            Line arcPath = new Line();
                            arcPath.X1 = k * 140 + 20;
                            arcPath.Y1 = ptCoord[key1];
                            col2 = t.Columns[c + 1].ColumnName;
                            String endpt = dr[c + 1].ToString();
                            String key2 = col2 + ":" + endpt;
                            arcPath.X2 = (k + 1) * 140 + 20;
                            arcPath.Y2 = ptCoord[key2];
                            arcPath.Stroke = new SolidColorBrush(Colors.Blue);
                            arcPath.StrokeThickness = 1;
                            arcPath.Fill = new SolidColorBrush(Colors.Blue);
                            arcPath.HorizontalAlignment = HorizontalAlignment.Center;
                            arcPath.ToolTip = key1 + "---" + key2;
                            userSel.Children.Add(arcPath);
                            c++;
                            k++;
                        }
                        if (attrmax < cols)
                        {
                            String stpt = dr[c].ToString();
                            col1 = t.Columns[c].ColumnName;
                            String key1 = col1 + ":" + stpt;
                            Line arcPath = new Line();
                            arcPath.X1 = (k) * 140 + 20;
                            arcPath.Y1 = ptCoord[key1];
                            col2 = t.Columns[globdecAttr].ColumnName;
                            String endpt = dr[decisionSel].ToString();
                            String key2 = col2 + ":" + endpt;
                            arcPath.X2 = (k + 1) * 140 + 20;
                            arcPath.Y2 = ptCoord[key2];
                            arcPath.Stroke = new SolidColorBrush(Colors.Blue);
                            arcPath.StrokeThickness = 1;
                            arcPath.Fill = new SolidColorBrush(Colors.Blue);
                            arcPath.HorizontalAlignment = HorizontalAlignment.Center;
                            arcPath.ToolTip = key1 + "---" + key2;
                            userSel.Children.Add(arcPath);
                        }
                        r++;
                        c = 0;
                    }
                    while (i1 < ((attrmax / 6) > 0 ? 6 : attrmax % 6))
                    {
                        drawAxesBrushed(i1, j1++);
                        i1++;
                    }

                    if (cols > 6 && attrmax < cols)
                    {
                        drawAxesBrushed(i1, decisionSel);
                        String temp = t.Columns[decisionSel].ColumnName; ;
                        Label Attr1 = new Label();
                        Attr1.Margin = new Thickness(850, 275, 0, 0);
                        Attr1.Visibility = Visibility.Visible;
                        Attr1.Content = temp;
                        Attr1.ToolTip = getDisVals(attrDisVal[temp]);
                        userSel.Children.Add(Attr1);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to plot the values", "Caution");
                //pcoord.Children.Clear();
                userSel.Children.Clear();
                //attrDDC.Children.Clear();
                //decAttr.Children.Clear();
            }
        }

        // Draws the plot on the canvas
        private void drawPlot(int attrInd, int attrmax)
        {
            try
            {
                int r = 0, c = attrInd;
                int cols = t.Columns.Count;
                String col1 = "";
                String col2 = "";
                for (int i = attrInd, j = 0; i < attrmax; i++)
                    visiAxes(j++, c++, 0, pcoord, "");
                while (r < t.Rows.Count)
                {
                    c = attrInd;
                    int k = 0;
                    for (; c < (attrmax) - 1; )
                    {
                        String stpt = t.DefaultView[r][c].ToString();
                        col1 = t.Columns[c].ColumnName;
                        String key1 = col1 + ":" + stpt;
                        Line arcPath = new Line();
                        arcPath.X1 = k * 140 + 20;
                        arcPath.Y1 = ptCoord[key1];
                        col2 = t.Columns[c + 1].ColumnName;
                        String endpt = t.DefaultView[r][c + 1].ToString();
                        String key2 = col2 + ":" + endpt;
                        arcPath.X2 = (k + 1) * 140 + 20;
                        arcPath.Y2 = ptCoord[key2];
                        arcPath.Stroke = new SolidColorBrush(Colors.Black);
                        arcPath.StrokeThickness = 1;
                        arcPath.Fill = new SolidColorBrush(Colors.Blue);
                        arcPath.HorizontalAlignment = HorizontalAlignment.Center;
                        arcPath.ToolTip = key1 + "---" + key2;
                        pcoord.Children.Add(arcPath);
                        c++;
                        k++;
                    }
                    if (attrmax < cols)
                    {
                        String stpt = t.DefaultView[r][c].ToString();
                        col1 = t.Columns[c].ColumnName;
                        String key1 = col1 + ":" + stpt;
                        Line arcPath = new Line();
                        arcPath.X1 = (k) * 140 + 20;
                        arcPath.Y1 = ptCoord[key1];
                        col2 = t.Columns[decisionSel].ColumnName;
                        String endpt = t.DefaultView[r][decisionSel].ToString();
                        String key2 = col2 + ":" + endpt;
                        arcPath.X2 = (k + 1) * 140 + 20;
                        arcPath.Y2 = ptCoord[key2];
                        arcPath.Stroke = new SolidColorBrush(Colors.Black);
                        arcPath.StrokeThickness = 1;
                        arcPath.Fill = new SolidColorBrush(Colors.Blue);
                        arcPath.HorizontalAlignment = HorizontalAlignment.Center;
                        arcPath.ToolTip = key1 + "---" + key2;
                        pcoord.Children.Add(arcPath);
                    }
                    r++;
                    c = 0;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to plot the values", "Caution");
                pcoord.Children.Clear();
                userSel.Children.Clear();
                attrDDC.Children.Clear();
                decAttr.Children.Clear();
            }
        }

        // When the user changes the region of interest on the canvas
        private void slider_ValueChanged(object sender, 
                        System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            ScrollBar slider=(ScrollBar)sender; 
            slider.Value = Math.Round(e.NewValue);
            slider.ToolTip = slider.Value.ToString();
            invisiAxes(Math.Round(e.NewValue));
            int intCoordInd = Convert.ToInt32(slider.Value);
            int min=intCoordInd*6;
            int max=((intCoordInd+1)*6);
            max=max>t.Columns.Count?t.Columns.Count:max;
            int i = 0, j = min ;
            
            drawPlot(min,max);
            while (i < ((max / 6) > 0 ? 6 : max % 6))
            {
                drawAxes(i, j++);
                i++;
            }
            if (max < t.Columns.Count)
            {
                drawAxes(i, decisionSel);
                String temp = t.Columns[decisionSel].ColumnName; ;
                Label Attr1 = new Label();
                Attr1.Margin = new Thickness(850, 275, 0, 0);
                Attr1.Visibility = Visibility.Visible;
                Attr1.Content = temp;
                Attr1.ToolTip = getDisVals(attrDisVal[temp]);
                pcoord.Children.Add(Attr1);
            }
            globMinAttr = min;
            globMaxAttr = max;
            if(!brushedAV.Equals(""))
            {
                userSel.Children.Clear();
                drawBrushedPlot(brushedAV);
            }
        }
    }
}
