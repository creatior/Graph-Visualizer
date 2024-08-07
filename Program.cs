using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Graphs
{
    public static class Match
    {
        public static double degtorad(double deg)//градусы в радианы
        {
            return deg * Math.PI / 180;
        }

        public static double radtodeg(double rad)//радианы в градусы
        {
            return rad / Math.PI * 180;
        }

        public static double lengthdir_x(double len, double dir)//расстояние по X при передвижении по направлению
        {
            return len * Math.Cos(degtorad(dir));
        }

        public static double lengthdir_y(double len, double dir)//расстояние по Y при передвижении по направлению
        {
            return len * Math.Sin(degtorad(dir)) * (-1);
        }

        public static double point_direction(int x1, int y1, int x2, int y2)//угол направления между двумя точками 
        {
            return 180 - radtodeg(Math.Atan2(y1 - y2, x1 - x2));
        }

        public static double point_distance(int x1, int y1, int x2, int y2)//расстояние между двумя точками
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }
    }

    public class Graph
    {
        public class Node
        {
            public int id;
            public int active;
            public int x;
            public int y;
            public string name;
            public List<Tuple<int, int>> edges;

            public void AddEdge(int id, int weight)
            {
                edges.Add(new Tuple<int, int>(id, weight));
            }
        }

        public List<Node> nodes = new List<Node>();
        private int maxid = 0;
        public int x = 0;
        public int y = 0;
        public int size = 32;

        public Queue<int> nodes_q = new Queue<int>();

        public void AddNode(string name)
        {
            bool find = false;
            int id = 0;

            for (int i = 0; i < size; i++)
            {
                bool exist = false;
                foreach (Node node in nodes)
                {
                    if (node.id == i)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    id = i;
                    find = true;
                    break;
                }
            }
            if (!find)
            {
                id = maxid;
                maxid++;
            }
            Node n = new Node();
            n.id = id;
            n.x = x;
            n.y = y;

            if (name != "")
                n.name = name;
            else n.name = (id + 1).ToString();

            n.edges = new List<Tuple<int, int>>();
            nodes.Add(n);
            nodes.Sort((x, y) => x.id.CompareTo(y.id));
        }

        public void SaveGraphToFile()
        {
            StreamWriter sw = new StreamWriter("G.grf", false);
            foreach (Node node in nodes)
            {
                sw.Write(node.id + " " + node.name + " ");
                foreach (Tuple<int, int> edge in node.edges)
                {
                    sw.Write("{0} ", edge);
                }
                sw.WriteLine();
            }
            sw.Close();
        }

        public void LoadGraphFromFile()
        {
            StreamReader sr = new StreamReader("C:\\Users\\varte\\Desktop\\c+++\\c#\\LAB15\\bin\\Debug\\G.grf");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                Node node = new Node();
                string[] data = line.Split(new char[] { ' ', '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (data.Length == 2)
                {
                    AddNode(data[0]);
                    node.id = Convert.ToInt32(data[0]);
                    continue;
                }
                else
                {
                    node.id = Convert.ToInt32(data[0]);
                    node.name = data[1];
                    node.x = x;
                    node.y = y;
                    node.edges = new List<Tuple<int, int>>();
                    for (int i = 2; i < data.Length; i += 2)
                    {
                        node.edges.Add(new Tuple<int, int>(Convert.ToInt32(data[i]), Convert.ToInt32(data[i + 1])));
                    }
                    nodes.Add(node);
                }
            }
        }

        public List<int> BellmanFord()
        {
            if (nodes.Count == 0) return null;
            int[] dist = new int[nodes.Count];
            for (int i = 0; i < dist.Length; i++)
                dist[i] = int.MaxValue;
            dist[0] = 0;
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                foreach (Node n in nodes)
                {
                    foreach (var m in n.edges)
                    {
                        if (dist[n.id] != int.MaxValue)
                            if (dist[m.Item1] > dist[n.id] + m.Item2)
                                dist[m.Item1] = dist[n.id] + m.Item2;
                    }
                }
            }
            int start = -1;
            foreach (Node node in nodes)
            {
                foreach (var edge in node.edges)
                {
                    if (dist[node.id] != int.MaxValue &&
                    dist[edge.Item1] > dist[node.id] + edge.Item2)
                    {
                        start = node.id;
                    }
                }
            }
            if (start == -1) return null;
            else
            {
                foreach (Node node in nodes)
                {
                    if (node.id == start)
                    {
                        bool[] visited = new bool[nodes.Count];
                        List<int> cycle = new List<int>();
                        return FindNegativeCyclesFromVertexUtil(node, node, visited, cycle);
                    }
                }
            }
            return null;
        }

        public List<int> FindNegativeCyclesFromVertexUtil(Node start, Node v, bool[] visited, List<int> cycle)
        {
            visited[v.id] = true;
            cycle.Add(v.id);

            foreach (var i in v.edges)
            {
                if (!visited[i.Item1])
                {
                    foreach (Node node in nodes)
                        if (node.id == i.Item1)
                            return FindNegativeCyclesFromVertexUtil(start, node, visited, cycle);
                }
                if (i.Item1 == start.id && cycle.Count > 2)
                {
                    if (isNegativeCycle(cycle))
                        return cycle;
                }
                
            }
            cycle.RemoveAt(cycle.Count - 1);
            visited[v.id] = false;
            return null;
        }

        private bool isNegativeCycle(List<int> cycle)
        {
            int weight = 0;
            cycle.Add(cycle[0]);
            for (int i = 0; i < cycle.Count - 1; i++)
            {
                foreach (var node in nodes)
                {
                    if (node.id == cycle[i])
                    {
                        foreach (var m in node.edges)
                        {
                            if (m.Item1 == cycle[i + 1])
                                weight += m.Item2;
                        }
                    }
                }
            }
            return weight < 0;
        }
    }

    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
