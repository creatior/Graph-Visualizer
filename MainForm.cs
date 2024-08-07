using LAB15;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Graphs
{
    public partial class MainForm : Form
    {
        public static Graph graph = new Graph();

        public int drag = -1;
        public int drage = -1;

        public int dx1 = 0;
        public int dy1 = 0;
        public int dx2 = 0;
        public int dy2 = 0;

        public bool act = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            pictureBox1.Width = this.Width - 16;
            pictureBox1.Height = this.Height - pictureBox1.Location.Y - 39;
            graph.x = pictureBox1.Width / 2;
            graph.y = pictureBox1.Height / 2;

            Bitmap buffer = new Bitmap(Width, Height);
            Graphics gfx = Graphics.FromImage(buffer);

            SolidBrush myBrush1 = new SolidBrush(Color.Black);
            SolidBrush myBrush2 = new SolidBrush(Color.White);
            Pen myPen1 = new Pen(Color.Black);
            Pen myPen2 = new Pen(Color.Green);

            gfx.Clear(Color.White);
            myPen2.Color = Color.Green;
            myBrush2.Color = Color.Green;

            foreach (Graph.Node n in graph.nodes)
            {
                foreach (Tuple<int, int> edge in n.edges)
                {
                    foreach (Graph.Node m in graph.nodes)
                    {
                        if (m.id == edge.Item1)
                        {
                            double a = Match.point_direction(n.x, n.y, m.x, m.y);
                            double dist = Match.point_distance(n.x, n.y, m.x, m.y);
                            gfx.DrawLine(myPen2,
                                new Point(n.x + (int)Match.lengthdir_x(graph.size / 2, a), n.y + (int)Match.lengthdir_y(graph.size / 2, a)),
                                new Point(n.x + (int)Match.lengthdir_x(dist - (graph.size / 2), a),
                                n.y + (int)Match.lengthdir_y(dist - (graph.size / 2), a)));
                            gfx.DrawString(edge.Item2.ToString(), new Font("Arial", 12, FontStyle.Regular), myBrush1, new PointF((n.x + m.x) / 2, (n.y + m.y) / 2));
                            gfx.FillEllipse(myBrush2,
                                new Rectangle(n.x + (int)Match.lengthdir_x(dist - (graph.size / 2), a) - 4,
                                n.y + (int)Match.lengthdir_y(dist - (graph.size / 2), a) - 4, 8, 8));
                        }
                    }
                }
            }
            foreach (Graph.Node node in graph.nodes)
            {
                myBrush2.Color = Color.White;
                if (node.active == 1)
                    myBrush2.Color = Color.SteelBlue;
                if (node.active == 2)
                    myBrush2.Color = Color.Gray;
                if (node.active == 3)
                    myBrush2.Color = Color.Red;
                gfx.FillEllipse(myBrush2, new Rectangle(node.x - graph.size / 2, node.y - graph.size / 2, graph.size, graph.size));
                gfx.DrawEllipse(myPen1, new Rectangle(node.x - graph.size / 2, node.y - graph.size / 2, graph.size, graph.size));
                gfx.DrawString(node.name, new Font("Arial", graph.size / 4, FontStyle.Regular), myBrush1, new PointF(node.x - graph.size / 6, node.y - graph.size / 5));
            }
            if (drage != -1)
            {
                myBrush2.Color = Color.Green;
                double a1 = Match.point_direction(dx1, dy1, dx2, dy2);
                double dist1 = Match.point_distance(dx1, dy1, dx2, dy2);
                gfx.DrawLine(myPen2,
                    new Point(dx1 + (int)Match.lengthdir_x(graph.size / 2, a1), dy1 + (int)Match.lengthdir_y(graph.size / 2, a1)),
                    new Point(dx1 + (int)Match.lengthdir_x(dist1, a1), dy1 + (int)Match.lengthdir_y(dist1, a1)));
            }

            pictureBox1.Image = buffer;
            myBrush1.Dispose();
            myBrush2.Dispose();
            myPen1.Dispose();
            myPen2.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!act)
            {
                graph.AddNode(textBox1.Text);
                textBox1.Text = "";
            }
        }

            private void timer1_tick(object sender, EventArgs e)
        {
            graph.size = trackBar1.Value;
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                for (int j = 0; j < graph.nodes.Count; j++)
                {
                    if (i != j)
                    {
                        double dist = Match.point_distance(graph.nodes[i].x, graph.nodes[i].y, graph.nodes[j].x, graph.nodes[j].y);
                        int size_in = 10;
                        if (dist < (graph.size + size_in))
                        {
                            Random rnd = new Random();
                            if (graph.nodes[i].x == graph.nodes[j].x)
                            {
                                if (rnd.Next(2) == 1)
                                    graph.nodes[i].x += 1;
                                else
                                    graph.nodes[i].x += 1;
                            }
                            if (graph.nodes[i].y == graph.nodes[j].y)
                            {
                                if (rnd.Next(2) == 1)
                                    graph.nodes[i].y += 1;
                                else
                                    graph.nodes[i].y += 1;
                            }
                            if (graph.nodes[i].x < graph.nodes[j].x)
                            {
                                graph.nodes[i].x -= (int)(graph.size + size_in - dist);
                                graph.nodes[j].x += (int)(graph.size + size_in - dist);
                            }
                            else
                            {
                                graph.nodes[i].x += (int)(graph.size + size_in - dist);
                                graph.nodes[j].x -= (int)(graph.size + size_in - dist);
                            }
                            if (graph.nodes[i].y < graph.nodes[j].y)
                            {
                                graph.nodes[i].y -= (int)(graph.size + size_in - dist);
                                graph.nodes[j].y += (int)(graph.size + size_in - dist);
                            }
                            else
                            {
                                graph.nodes[i].y += (int)(graph.size + size_in - dist);
                                graph.nodes[j].y -= (int)(graph.size + size_in - dist);
                            }
                        }
                    }

                    if (graph.nodes[i].x - graph.size / 2 < 0) graph.nodes[i].x = graph.size / 2;
                    if (graph.nodes[i].y - graph.size / 2 < 0) graph.nodes[i].y = graph.size / 2;
                    if (graph.nodes[i].x + graph.size / 2 > pictureBox1.Width) graph.nodes[i].x = pictureBox1.Width - graph.size / 2 - 1;
                    if (graph.nodes[i].y + graph.size / 2 > pictureBox1.Height) graph.nodes[i].y = pictureBox1.Height - graph.size / 2 - 1;
                }

            }
            Refresh();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (graph.nodes_q.Count != 0)
            {
                int aid = graph.nodes_q.Dequeue();
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    if (graph.nodes[i].id == aid)
                    {
                        graph.nodes[i].active = 2;
                        foreach (var edge in graph.nodes[i].edges)
                        {
                            foreach (Graph.Node m in graph.nodes)
                            {
                                if (m.id == edge.Item1)
                                {
                                    if (m.active == 0)
                                    {
                                        m.active = 1;
                                        graph.nodes_q.Enqueue(m.id);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    if (graph.nodes[i].active == 0)
                    {
                        graph.nodes[i].active = 1;
                        graph.nodes_q.Enqueue(graph.nodes[i].id);
                        break;
                    }
                }
                if (graph.nodes_q.Count == 0)
                {
                    foreach (Graph.Node n in graph.nodes)
                        n.active = 0;
                    timer2.Stop();
                    act = false;
                }
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag != -1)
            {
                foreach (Graph.Node node in graph.nodes)
                {
                    if (drag == node.id)
                    {
                        node.x = e.X;
                        node.y = e.Y;
                        break;
                    }
                }
            }
            if (drage != -1)
            {
                foreach (Graph.Node node in graph.nodes)
                {
                    if (drage == node.id)
                    {
                        dx1 = node.x;
                        dy1 = node.y;
                        dx2 = e.X;
                        dy2 = e.Y;
                        break;
                    }
                }
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                drage = -1;
                if (drag == -1)
                {
                    foreach (Graph.Node node in graph.nodes)
                    {
                        if (Match.point_distance(node.x, node.y, e.X, e.Y) < graph.size / 2)
                        {
                            drag = node.id;
                            node.x = e.X;
                            node.y = e.Y;
                            break;
                        }
                    }
                }
            }
            if (!act)
            {
                if (e.Button == MouseButtons.Right)
                {
                    drag = -1;
                    dx1 = 0;
                    dy1 = 0;
                    dx2 = 0;
                    dy2 = 0;
                    foreach (Graph.Node node in graph.nodes)
                    {
                        if (Match.point_distance(node.x, node.y, e.X, e.Y) < graph.size / 2)
                        {
                            drage = node.id;
                            dx1 = node.x;
                            dy1 = node.y;
                            dx2 = e.X;
                            dy2 = e.Y;
                            break;
                        }
                    }
                }
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                drag = -1;
            if (e.Button == MouseButtons.Right)
            {
                if (drage != -1)
                {
                    foreach (Graph.Node node in graph.nodes)
                    {
                        if (Match.point_distance(node.x, node.y, e.X, e.Y) < graph.size / 2)
                        {
                            if (node.id != drage)
                            {
                                foreach (Graph.Node m in graph.nodes)
                                {
                                    if (m.id == drage)
                                    {
                                        if (Int32.TryParse(textBox2.Text, out int weight) == false)
                                        {
                                            m.AddEdge(node.id, 1);
                                            //node.AddEdge(m.id, 1);
                                        }
                                        else
                                        {
                                            m.AddEdge(node.id, weight);
                                            //node.AddEdge(m.id, weight);
                                        }
                                        textBox2.Text = "";
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                drage = -1;
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            graph.SaveGraphToFile();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            graph = new Graph();
            graph.LoadGraphFromFile();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<int> negative_cycle = graph.BellmanFord();
            if (negative_cycle == null)
            {
                label2.Text = "В графе нет отрицательных циклов";
                return;
            }
            label2.Text = "";
            foreach (int vertex in negative_cycle)
            {
                foreach (Graph.Node node in graph.nodes)
                {
                    if (node.id == vertex)
                        node.active = 3;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer2.Interval = 1000;
            drag = -1;
            drage = -1;
            
            graph.nodes_q.Clear();
            if (!timer2.Enabled && !act)
            {
                if (graph.nodes.Count > 0)
                {
                    graph.nodes[0].active = 1;
                    graph.nodes_q.Enqueue(graph.nodes[0].id);
                    timer2.Start();
                    act = true;
                }
            }
            else
            {
                timer2.Stop();
                act = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void clear_graph_button_Click(object sender, EventArgs e)
        {
            ClearApplyForm form = new ClearApplyForm();
            form.Show();
        }
    }
}
