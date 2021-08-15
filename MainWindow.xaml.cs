using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CalculatorSolutionsObserver.Utils;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;

namespace CalculatorSolutionsObserver
{
    public partial class MainWindow : Window
    {
        public GraphViewer graphViewer = new GraphViewer();
        public List<OperationEgg> operationEggs = new List<OperationEgg>(new OperationEgg[] {
            new OperationEgg("+", (value, operand) => value+operand),
            new OperationEgg("-", (value, operand) => value-operand),
            new OperationEgg("*", (value, operand) => value*operand),
            new OperationEgg("/", (value, operand) => { 
                if (value % operand != 0) throw new InvalidOperationException(); 
                return value / operand; 
            }),
            new OperationEgg((value, operand) => value * 2, "Сделать четным (x*2)"),
            new OperationEgg((value, operand) => value * 2 + 1, "Сделать нечетным (x*2+1)"),
            new OperationEgg((value, operand) => value + (value-1), "Прибавить предыдущее (x+(x-1)"),
            new OperationEgg((value, operand) => value - (value-1), "Отнять предыдущее (x-(x-1)")
        });

        public List<Operation> operations = new List<Operation>();
        private int start, end;

        public MainWindow()
        {
            InitializeComponent();
            graphViewer.BindToPanel(graphViewerPanel);
            OperationEggsField.ItemsSource = operationEggs;
            OperationEggsField.Items.Refresh();
            OperationsField.ItemsSource = operations;
        }

        private void ReloadOperations()
        {
            OperationsField.Items.Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void AddOperationButton_Click(object sender, RoutedEventArgs e)
        {
            if (OperationEggsField.SelectedItem == null) return;

            var egg = OperationEggsField.SelectedItem as OperationEgg;
            if (egg.IsOperandRequired)
            {
                if (int.TryParse(OperandField.Text, out int operand))
                {
                    operations.Add(egg.Hatch(operand));
                }
                else
                {
                    MessageBox.Show(
                        "Операнд не является числом!",
                        "Внимание",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }
            }
            else
            {
                operations.Add(egg.Hatch());
            }

            ReloadOperations();
        }

        private void OperationEggsField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OperationEggsField.SelectedItem == null)
            {
                OperandField.IsEnabled = false;
                AddOperationButton.IsEnabled = false;
                return;
            }

            var egg = OperationEggsField.SelectedItem as OperationEgg;

            OperandField.IsEnabled = egg.IsOperandRequired;
            AddOperationButton.IsEnabled = true;
        }

        private void OperationsField_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OperationsField.SelectedItem == null) return;
            operations.RemoveAt(OperationsField.SelectedIndex);
            ReloadOperations();
        }

        private void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(startField.Text, out start) || 
                !int.TryParse(endField.Text, out end))
            {
                MessageBox.Show(
                    "Стартовое и (или) конечное значение не являются допустимыми числами.",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var tree = new NTree<int, int>();
            tree.Value = start;

            var graph = new Graph();

            process(tree, out int solutionsCount);

            CreateGraph(ref graph, tree);
            DrawGraph(graph);
            MessageBox.Show(solutionsCount.ToString());
        }

        private void DrawGraph(Graph graph)
        {
            graphViewer.Graph = graph;
        }

        private void CreateGraph(ref Graph graph, NTree<int, int> currentNode, Node parentNode = null)
        {
            if (parentNode == null)
            {
                parentNode = new Node(Guid.NewGuid().ToString());
                parentNode.LabelText = currentNode.Value.ToString();
                parentNode.Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
                parentNode.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Hexagon;
                graph.AddNode(parentNode);
                
            }

            foreach (NTree<int, int> child in currentNode.Children)
            {
                if (child.LinkValue > 0)
                {
                    Edge edge = graph.AddEdge(parentNode.Id, child.LinkValue.ToString(), Guid.NewGuid().ToString());
                    edge.TargetNode.LabelText = child.Value.ToString();
                    if (child.Value == end)
                    {
                        edge.TargetNode.Attr.Shape = Microsoft.Msagl.Drawing.Shape.DoubleCircle;
                        edge.TargetNode.Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                    }
                    CreateGraph(ref graph, child, edge.TargetNode);
                }
            }
        }

        private NTree<int, int> process(NTree<int, int> node, out int linkValue)
        {
            linkValue = 0;

            if (start == end) return node;

            foreach (Operation operation in operations)
            {
                var currentNode = new NTree<int, int>();
                try
                {
                    currentNode.Value = operation.Execute(node.Value);
                }
                catch (Exception e)
                {
                    continue;
                }
                

                if (currentNode.Value == end)
                {
                    linkValue++;
                    currentNode.LinkValue = 1;
                    node.AddChild(currentNode);
                    continue;
                }

                if ((currentNode.Value < start && currentNode.Value > end) || 
                    (currentNode.Value > start && currentNode.Value < end))
                {
                    process(currentNode, out int childLinkValue);
                    linkValue += childLinkValue;
                    node.AddChild(currentNode);
                }
            }

            node.LinkValue = linkValue;

            return node;
        }
    }
}
