using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            process(tree);
        }

        private void DrawGraph()
        {
            Graph graph = new Graph();
            
        }

        private NTree<int, int> process(NTree<int, int> node)
        {
            if (start == end) return node;

            foreach(Operation operation in operations)
            {
                var currentNode = new NTree<int, int>();
                currentNode.Value = operation.Execute(node.Value);
                
                if ((currentNode.Value < start && currentNode.Value > end) || (currentNode.Value > start && currentNode.Value < end))
                {
                    process(currentNode);
                }

                node.AddChild(currentNode);
            }

            return node;
        }
    }
}
