using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoordinateConversion
{
    public partial class Form1 : Form
    {
        double initial = 999999999999999;
        int sign = 0, sign1 = 0, sign2 = 0;
        Ellispoid E = new Ellispoid();
        List<PointBLH> LpBLH = new List<PointBLH>();
        List<PointXYZ> LpXYZ = new List<PointXYZ>();
        List<PointBLL0> LpBLL0 = new List<PointBLL0>();
        List<PointXYL0> LpXYL0 = new List<PointXYL0>();
        private string title = "坐标转换程序";

        public Form1()
        {
            InitializeComponent();
            this.Text = title;
        }

        private void 打开OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opdg = new OpenFileDialog();
            opdg.Filter = "坐标数据文件|*.txt|所有文件|*.*";
            if (opdg.ShowDialog() != DialogResult.OK)
                return;
            else
            {
                string filename = opdg.FileName;
                this.Text = title + "-" + filename;
                richTextBox1.LoadFile(filename, RichTextBoxStreamType.PlainText);
                boxToList();
                sign = 1;
            }
        }

        private int boxToList()
        {
            string s1 = richTextBox1.Text;
            s1 = s1.Trim();
            s1 = s1.Replace(" ", "");
            s1 = s1.Replace("\t", "");
            string[] s2 = s1.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string[]> s3 = new List<string[]>();
            foreach(string tmp in s2)
            {
                s3.Add(tmp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }
            string[][] s4 = s3.ToArray();
            E.a = double.Parse(s4[0][1]);
            E.f = 1/double.Parse(s4[1][1]);
            double L0 = double.Parse(s4[2][1]);
            int row = s4.GetLength(0);
            for (int i = 3; i < row; i++)
            {
                Conversion convert = new Conversion();
                PointBLH p = new PointBLH();
                PointBLL0 p0 = new PointBLL0();
                p.Name = s4[i][0];
                p.B = convert.Angle2Radian(double.Parse(s4[i][1]));
                p.L = convert.Angle2Radian(double.Parse(s4[i][2]));
                p.H = double.Parse(s4[i][3]);
                p0.Name = s4[i][0];
                p0.B = convert.Angle2Radian(double.Parse(s4[i][1]));
                p0.L = convert.Angle2Radian(double.Parse(s4[i][2]));
                p0.L0 = convert.Angle2Radian(L0);
                LpBLH.Add(p);
                LpBLL0.Add(p0);
            }
            return 0;
        }

        private void 大地坐标转空间直角坐标ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sign == 1)
            {
                LpXYZ.Clear();
                Conversion convert = new Conversion();
                int count = LpBLH.Count;
                for (int i = 0; i < count; i++)
                {
                    PointXYZ xyz = new PointXYZ();
                    convert.CalculateEllispoid(ref E, LpBLH[i].B);
                    convert.BLH2XYZ(LpBLH[i], E, ref xyz);
                    LpXYZ.Add(xyz);
                }
                int style = 1;
                writeListView(style);
                style = 11;
                writeFile(style);
                sign1 = 1;
            }
        }

        private void 空间直角坐标XYZ转大地坐标BLHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sign == 1)
            {
                if (sign1 == 1)
                {
                    LpBLH.Clear();
                    Conversion convert = new Conversion();
                    int count = LpXYZ.Count;
                    for (int i = 0; i < count; i++)
                    {
                        PointBLH blh = new PointBLH();
                        convert.XYZ2BLH(LpXYZ[i], E.a, E.f, ref blh);
                        LpBLH.Add(blh);
                    }
                    int style = 1;
                    writeListView(style);
                    style = 12;
                    writeFile(style);
                }
                else
                    MessageBox.Show("错误：请先进行 “大地坐标(B,L,H)转空间直角坐标(X,Y,Z)” 操作！");
            }
        }

        private void 高斯投影正算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sign == 1)
            {
                LpXYL0.Clear();
                Conversion convert = new Conversion();
                int count = LpBLL0.Count;
                for (int i = 0; i < count; i++)
                {
                    PointXYL0 xyl0 = new PointXYL0();
                    convert.BL2XY(LpBLL0[i], E, ref xyl0);
                    LpXYL0.Add(xyl0);
                }
                int style = 2;
                writeListView(style);
                pictureBox1.Refresh();
                style = 21;
                writeFile(style);
                sign2 = 1;
            }
        }

        private void 高斯投影反算ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sign == 1)
            {
                if (sign2 == 1)
                {
                    LpBLL0.Clear();
                    Conversion convert = new Conversion();
                    int count = LpXYL0.Count;
                    for (int i = 0; i < count; i++)
                    {
                        PointBLL0 bll0 = new PointBLL0();
                        convert.XY2BL(LpXYL0[i], E, ref bll0);
                        LpBLL0.Add(bll0);
                    }
                    int style = 2;
                    writeListView(style);
                    pictureBox1.Refresh();
                    style = 22;
                    writeFile(style);
                }
                else
                    MessageBox.Show("错误：请先进行 “高斯投影正算(B,L)转(X,Y)” 操作！");
            }
        }

        private void writeListView(int style)
        {
            Conversion convert = new Conversion();
            listView1.Items.Clear();
            switch(style)
            {
                case 1:
                    int countBLH = LpBLH.Count;
                    for (int i = 0; i < countBLH; i++)
                    {
                        ListViewItem line = new ListViewItem();
                        line.Text = (listView1.Items.Count + 1).ToString();
                        line.SubItems.Add(LpBLH[i].Name);
                        line.SubItems.Add(convert.Angle2DMS(convert.Radian2Angle(LpBLH[i].B)));
                        line.SubItems.Add(convert.Angle2DMS(convert.Radian2Angle(LpBLH[i].L)));
                        line.SubItems.Add(LpBLH[i].H.ToString("0.0000"));
                        line.SubItems.Add(LpXYZ[i].X.ToString("0.0000"));
                        line.SubItems.Add(LpXYZ[i].Y.ToString("0.0000"));
                        line.SubItems.Add(LpXYZ[i].Z.ToString("0.0000"));
                        line.SubItems.Add("/");
                        line.SubItems.Add("/");
                        listView1.Items.Add(line);
                    }
                    break;
                case 2:
                    int countBLL0 = LpBLL0.Count;
                    for (int i = 0; i < countBLL0; i++)
                    {
                        ListViewItem line = new ListViewItem();
                        line.Text = (listView1.Items.Count + 1).ToString();
                        line.SubItems.Add(LpBLL0[i].Name);
                        line.SubItems.Add(convert.Angle2DMS(convert.Radian2Angle(LpBLL0[i].B)));
                        line.SubItems.Add(convert.Angle2DMS(convert.Radian2Angle(LpBLL0[i].L)));
                        line.SubItems.Add("/");
                        line.SubItems.Add(LpXYL0[i].X.ToString("0.0000"));
                        line.SubItems.Add(LpXYL0[i].Y.ToString("0.0000"));
                        line.SubItems.Add("/");
                        line.SubItems.Add(convert.Angle2DMS(convert.Radian2Angle(LpBLL0[i].L0)));
                        line.SubItems.Add("/");
                        listView1.Items.Add(line);
                    }
                    break;
                default:
                    break;
            }
        }

        private double Naturalization(double value, double maxBox, double minBox, double max, double min)
        {
            return (maxBox - minBox) / (max - min) * (value - min) + minBox;
        }

        private int findMaxAndMin(List<PointXYL0> xyl0, ref double maxX, ref double minX, ref double maxY, ref double minY)
        {
            int countXYL0 = xyl0.Count;
            for (int i = 0; i < countXYL0; i++)
            {
                if (maxX < xyl0[i].X)
                    maxX = xyl0[i].X;
                if (minX > xyl0[i].X)
                    minX = xyl0[i].X;
                if (maxY < xyl0[i].Y)
                    maxY = xyl0[i].Y;
                if (minY > xyl0[i].Y)
                    minY = xyl0[i].Y;
            }
            return 0;
        }

        private void pictureBox1_Paint_1(object sender, PaintEventArgs e)
        {
            double maxX = -initial, minX = initial;
            double maxY = -initial, minY = initial;
            findMaxAndMin(LpXYL0, ref maxX, ref minX, ref maxY, ref minY);
            double x, y, dx, dy;
            int countXYL0 = LpXYL0.Count;
            double width = pictureBox1.Width / 1.5;
            double height = pictureBox1.Height / 1.5;
            dy = width / 4.0;
            dx = height / 4.0;
            SolidBrush brush = new SolidBrush(Color.Red);
            Pen pen = new Pen(Color.Red);
            Font font = new Font("宋体", 8, FontStyle.Regular);
            Graphics g = e.Graphics;
            Bitmap b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics dc = Graphics.FromImage((System.Drawing.Image)b);

            for (int i = 0; i < countXYL0; i++)
            {
                x = height - Naturalization(LpXYL0[i].X, height, 0, maxX, minX) + dx;
                y = Naturalization(LpXYL0[i].Y, width, 0, maxY, minY) + dy;
                Point[] point = new Point[3];
                point[0] = new Point(Convert.ToInt32(y), Convert.ToInt32(x) - 5);
                point[1] = new Point(Convert.ToInt32(y) - 5, Convert.ToInt32(x) + 5);
                point[2] = new Point(Convert.ToInt32(y) + 5, Convert.ToInt32(x) + 5);
                dc.FillPolygon(brush, point);
                dc.DrawString(LpXYL0[i].Name, font, brush, Convert.ToInt32(y) - 5, Convert.ToInt32(x) + 8);
            }
            g.DrawImage(b, 0, 0);
            dc.Dispose();
        }

        private void writeFile(int style)
        {
            Conversion convert = new Conversion();
            switch (style)
            {
                case 11:
                    string filePath11 = "C:\\Users\\jone\\Desktop\\测量省赛\\大地坐标转换报告.txt";
                    if (File.Exists(filePath11))
                        File.Delete(filePath11);
                    FileStream fs11 = new FileStream(filePath11, FileMode.Create);
                    byte[] data11 = null;
                    string filetitle11 = "大地坐标(B, L, H)转换为空间坐标(X, Y, Z)\n"
                        + "------------------------------------------------------------------------\n";
                    data11 = System.Text.Encoding.Default.GetBytes(filetitle11 + "点名\tB\tL\tH\tX\tY\tZ\n");
                    fs11.Write(data11, 0, data11.Length);
                    for (int i = 0; i < LpBLH.Count; i++)
                    {
                        string content = string.Format("{0}  {1}  {2}  {3}  {4}  {5}  {6}\n",
                            LpBLH[i].Name, convert.Angle2DMS(convert.Radian2Angle(LpBLH[i].B)),
                            convert.Angle2DMS(convert.Radian2Angle(LpBLH[i].L)), LpBLH[i].H.ToString("0.0000"),
                            LpXYZ[i].X.ToString("0.0000"), LpXYZ[i].Y.ToString("0.0000"), LpXYZ[i].Z.ToString("0.0000"));
                        data11 = System.Text.Encoding.Default.GetBytes(content);
                        fs11.Write(data11, 0, data11.Length);
                    }
                    fs11.Flush();
                    fs11.Close();
                    MessageBox.Show("坐标转换成功！\n转换结果保存在：\n\t“大地坐标转换报告.txt”");
                    break;
                case 12:
                    string filePath12 = "C:\\Users\\jone\\Desktop\\测量省赛\\空间坐标转换报告.txt";
                    if (File.Exists(filePath12))
                        File.Delete(filePath12);
                    FileStream fs12 = new FileStream(filePath12, FileMode.Create);
                    byte[] data12 = null;
                    string filetitle12 = "空间坐标(X, Y, Z)转换为大地坐标(B, L, H)\n"
                        + "------------------------------------------------------------------------\n";
                    data12 = System.Text.Encoding.Default.GetBytes(filetitle12 + "点名\tX\tY\tZ\tB\tL\tH\n");
                    fs12.Write(data12, 0, data12.Length);
                    for (int i = 0; i < LpXYZ.Count; i++)
                    {
                        string content = string.Format("{0}  {1}  {2}  {3}  {4}  {5}  {6}\n",
                            LpBLH[i].Name, LpXYZ[i].X.ToString("0.0000"), LpXYZ[i].Y.ToString("0.0000"),
                            LpXYZ[i].Z.ToString("0.0000"), convert.Angle2DMS(convert.Radian2Angle(LpBLH[i].B)),
                            convert.Angle2DMS(convert.Radian2Angle(LpBLH[i].L)), LpBLH[i].H.ToString("0.0000"));
                        data12 = System.Text.Encoding.Default.GetBytes(content);
                        fs12.Write(data12, 0, data12.Length);
                    }
                    fs12.Flush();
                    fs12.Close();
                    MessageBox.Show("坐标转换成功！\n转换结果保存在：\n\t“空间坐标转换报告.txt”");
                    break;
                case 21:
                    string filePath21 = "C:\\Users\\jone\\Desktop\\测量省赛\\高斯投影正算报告.txt";
                    if (File.Exists(filePath21))
                        File.Delete(filePath21);
                    FileStream fs21 = new FileStream(filePath21, FileMode.Create);
                    byte[] data21 = null;
                    string filetitle21 = "高斯投影正算(B, L)转换为(X, Y)\n"
                        + "------------------------------------------------------------------------\n";
                    data21 = System.Text.Encoding.Default.GetBytes(filetitle21 + "点名\tB\tL\tX\tY\tL0\n");
                    fs21.Write(data21, 0, data21.Length);
                    for (int i = 0; i < LpBLL0.Count; i++)
                    {
                        string content = string.Format("{0}  {1}  {2}  {3}  {4}  {5}\n",
                            LpBLL0[i].Name, convert.Angle2DMS(convert.Radian2Angle(LpBLL0[i].B)),
                            convert.Angle2DMS(convert.Radian2Angle(LpBLL0[i].L)), LpXYL0[i].X.ToString("0.0000"),
                            LpXYL0[i].Y.ToString("0.0000"), convert.Angle2DMS(convert.Radian2Angle(LpBLL0[i].L0)));
                        data21 = System.Text.Encoding.Default.GetBytes(content);
                        fs21.Write(data21, 0, data21.Length);
                    }
                    fs21.Flush();
                    fs21.Close();
                    MessageBox.Show("坐标转换成功！\n转换结果保存在：\n\t“高斯投影正算报告.txt”");
                    break;
                case 22:
                    string filePath22 = "C:\\Users\\jone\\Desktop\\测量省赛\\高斯投影反算报告.txt";
                    if (File.Exists(filePath22))
                        File.Delete(filePath22);
                    FileStream fs22 = new FileStream(filePath22, FileMode.Create);
                    byte[] data22 = null;
                    string filetitle22 = "高斯投影反算(X, Y)转换为(B, L)\n"
                        + "------------------------------------------------------------------------\n";
                    data22 = System.Text.Encoding.Default.GetBytes(filetitle22 + "点名\tX\tY\tB\tL\tL0\n");
                    fs22.Write(data22, 0, data22.Length);
                    for (int i = 0; i < LpXYL0.Count; i++)
                    {
                        string content = string.Format("{0}  {1}  {2}  {3}  {4}  {5}\n",
                            LpBLL0[i].Name, LpXYL0[i].X.ToString("0.0000"), LpXYL0[i].Y.ToString("0.0000"),
                            convert.Angle2DMS(convert.Radian2Angle(LpBLL0[i].B)),
                            convert.Angle2DMS(convert.Radian2Angle(LpBLL0[i].L)), 
                            convert.Angle2DMS(convert.Radian2Angle(LpXYL0[i].L0)));
                        data22 = System.Text.Encoding.Default.GetBytes(content);
                        fs22.Write(data22, 0, data22.Length);
                    }
                    fs22.Flush();
                    fs22.Close();
                    MessageBox.Show("坐标转换成功！\n转换结果保存在：\n\t“高斯投影反算报告.txt”");
                    break;
                default:
                    break;
            }
        }
    }
}
