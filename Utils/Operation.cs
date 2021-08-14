using System;
using System.Collections.Generic;
using System.Text;

namespace CalculatorSolutionsObserver.Utils
{
    public delegate int OperationCapsule(int value, int operand);

    public class Operation
    {
        public int Operand = 0;
        public string Nicename { get; set; }

        public OperationCapsule Lambda;

        public Operation(int Operand, OperationCapsule Lambda)
        {
            this.Operand = Operand;
            this.Lambda = Lambda;
        }

        public Operation(int Operand, string Nicename, OperationCapsule Lambda)
        {
            this.Nicename = Nicename;
            this.Operand = Operand;
            this.Lambda = Lambda;
        }

        public Operation(string Nicename, OperationCapsule Lambda)
        {
            this.Nicename = Nicename;
            this.Lambda = Lambda;
        }

        public int Execute(int Value)
        {
            return Lambda(Value, Operand);
        }
    }

    public class OperationEgg
    {
        private string Operator;
        private string Nicename;
        private OperationCapsule Lambda;
        public bool IsOperandRequired;

        public override string ToString()
        {
            return Nicename ?? Operator;
        }

        public OperationEgg(string Operator, OperationCapsule Lambda, bool IsOperandRequired = true)
        {
            this.Operator = Operator;
            this.Lambda = Lambda;
            this.IsOperandRequired = IsOperandRequired;
        }

        public OperationEgg(OperationCapsule Lambda, string Nicename, bool IsOperandRequired = false)
        {
            this.Nicename = Nicename;
            this.Lambda = Lambda;
            this.IsOperandRequired = IsOperandRequired;
        }

        public Operation Hatch()
        {
            return new Operation(ToString(), Lambda);
        }

        public Operation Hatch(int Operand)
        {
            return new Operation(Operand, Nicename ?? (Operator + " " + Operand.ToString()), Lambda);
        }
    }
}
