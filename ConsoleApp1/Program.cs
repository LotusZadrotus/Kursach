using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using NMath.MathObjects;
using NMath;
namespace ConsoleApp1
{
    class Program
    {
        public struct Equation
        {
            private Vector solve;
            private Matrix matrix;
            private string type;
            public string Type
            {
                get => type;
            }
            public Matrix Nums
            {
                get => matrix;
                set => matrix = value;
            }
            private Vector vector;

            public Vector Vector
            {
                get => vector;
                set => vector = value;
            }
            public Vector SolveVector
            {
                get => solve;
                set => solve = value;
            }

            public Equation(Matrix Nums, Vector vector, Vector solve)
            {
                this.solve = solve;
                this.matrix = Nums;
                this.vector = vector;
                Type type = Nums.GetType();
                if (type == typeof(IdentityMatrix))
                    this.type = "Identity";
                else if (type == typeof(HilbertMtrx))
                    this.type = "Hilbert";
                else 
                    this.type = "Not a Special"; 
            }
        }
        static Dictionary<int, Equation> Equations = new Dictionary<int, Equation>();
        private static Dictionary<string, Action<string[]>> actions = new Dictionary<string, Action<string[]>>()
        {
            {
                "/exit", (x) => { System.Environment.Exit(0); }
            },
            {
                "/read", (x) =>
                {
                    string a = x[1];
                    using TextReader textReader = new StreamReader(a);
                    try
                    {
                        int k = 0;
                        string[] args = textReader.ReadLine()
                            ?.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var s in args)
                        {
                            k++;
                        }

                        double[,] aDoubles = new double[k - 1, k - 1];
                        double[] bDoubles = new double[k - 1];
                        for (int i = 0; i < k - 1; i++)
                        {
                            aDoubles[0, i] = Convert.ToDouble(args[i]);
                        }

                        bDoubles[0] = Convert.ToDouble(args[k - 1]);
                        for (int i = 1; i < k - 1; i++)
                        {

                            string[] args1 = textReader.ReadLine()
                                ?.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                            for (int j = 0; j < k - 1; j++)
                            {
                                aDoubles[i, j] = Convert.ToDouble(args1[j]);
                            }

                            bDoubles[i] = Convert.ToDouble(args1[k - 1]);
                        }

                        Matrix matrix = new Matrix(aDoubles);
                        Vector vector = new Vector(bDoubles);
                        Console.WriteLine(matrix);
                        Equations.Add((Equations.Count), new Equation(matrix, vector, new Vector(vector.Size)));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);

                    }
                    textReader.Close();
                }
            },
            {
                "/save", (x) => { }
            },
            {
                "/solve", (x) =>
                {
                    Vector sol=null;
                    Equation equation = Equations[Convert.ToInt32(x[1])];
                    switch (x[2])
                    {
                        case "-g":
                            if (x.Length>3)
                            {
                                Matrix i = new IdentityMatrix(equation.Vector.Size);
                                double a = Convert.ToDouble(x[4]);
                                sol=NMath.NMath.Equation.SolveGaussWithMainElementsChoosing(equation.Nums+a*i,equation.Vector);
                                break;
                            }
                            sol=NMath.NMath.Equation.SolveGaussWithMainElementsChoosing(equation.Nums,equation.Vector);
                            break;
                        case "-z":
                            if (x.Length>3)
                            {
                                switch (x[3])
                                {
                                    case "-reg":
                                        Matrix i = new IdentityMatrix(equation.Vector.Size);
                                        double a = Convert.ToDouble(x[4]);
                                        sol=NMath.NMath.Equation.SeidelIteration(equation.Nums+a*i,equation.Vector);
                                        break;
                                    case "-pred":
                                        // if (x.Length>4)
                                        // {
                                        //     Matrix k = new IdentityMatrix(equation.Vector.Size);
                                        //     double j = Convert.ToDouble(x[4]);
                                        //     Matrix matrix3 = getTriangle(equation.Nums+j*k);
                                        //     matrix3 = NMath.NMath.MatrixOperation.MtrxProduct.Invert(matrix3);
                                        //     var g5 = new Vector(equation.Vector);
                                        //     g5.Transpose();
                                        //     sol=NMath.NMath.Equation.SeidelIteration(matrix3*(equation.Nums+j*k),(Vector)(g5*matrix3));
                                        //     break;
                                        // }
                                        Matrix matrix = getTriangle(equation.Nums);
                                        matrix = NMath.NMath.MatrixOperation.MtrxProduct.Invert(matrix);
                                        var g = new Vector(equation.Vector);
                                        g.Transpose();
                                        sol=NMath.NMath.Equation.SeidelIteration(matrix*equation.Nums,(Vector)(g*matrix));
                                        break;
                                }

                                break;
                            }
                            sol=NMath.NMath.Equation.SeidelIteration(equation.Nums, equation.Vector);
                            break;
                        case "-r":
                            double omega = Convert.ToDouble(x[3]);
                            if (x.Length>4)
                            {
                                switch (x[4])
                                {
                                    case "-reg":
                                        Matrix i = new IdentityMatrix(equation.Vector.Size);
                                        double a = Convert.ToDouble(x[5]);
                                        sol=NMath.NMath.Equation.Rel(equation.Nums+a*i,equation.Vector, omega);
                                        break;
                                    case "-pred":
                                        Matrix matrix = getTriangle(equation.Nums);
                                        matrix = NMath.NMath.MatrixOperation.MtrxProduct.Invert(matrix);
                                        var g = new Vector(equation.Vector);
                                        g.Transpose();
                                        sol=NMath.NMath.Equation.Rel(matrix*equation.Nums,(Vector)(g*matrix),omega);
                                        break;
                                }
                                break;
                            }
                            sol=NMath.NMath.Equation.Rel(equation.Nums, equation.Vector, omega);
                            break;
                        case "-p":
                            Vector x0=new Vector(equation.Vector.Size);
                            if (x.Length>3)
                            {
                                Matrix i = new IdentityMatrix(equation.Vector.Size);
                                double a = Convert.ToDouble(x[3]);
                                sol=NMath.NMath.Equation.BCG(equation.Nums+a*i,equation.Vector,x0);
                                break;
                            }
                            sol = NMath.NMath.Equation.BCG(equation.Nums, equation.Vector,x0);
                            break;
                        default:
                            Console.WriteLine("Use '-g', '-z', '-p' to choose a method of solving.");
                            return;
                    }
                    var u = new IdentityVector(sol.Size);
                    Console.WriteLine($"Solve vector: \n\t{sol}");
                    Console.WriteLine($"Nevyazka: \t{((Vector)(sol)-equation.SolveVector).Norm(2)}");
                    // Console.WriteLine($"Nevyazka: \t{(Vector)(equation.Nums*sol)}");
                }
            },
            {
                "/show", (x) =>
                {

                    try
                    {
                        foreach (var equation in Equations)
                        {
                            Console.WriteLine($"\t{equation.Key} {equation.Value.Type} matrix n:{equation.Value.Vector.Size}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            },
            {
                "/create", (x) =>
                {
                    try
                    {
                        Random rnd = new Random();
                        Matrix h = new HilbertMtrx(Convert.ToInt32(x[1]));
                        Vector x1 = new Vector(Convert.ToInt32(x[1]));
                        for (int i = 0; i < x1.Size; i++)
                        {
                            x1[i] = Convert.ToDouble(rnd.Next(0,10));
                        }
                        Equations.Add((Equations.Count), new Equation(h, (Vector)(h*x1),x1));
                        Console.WriteLine($"Created a new equation with Hilbert matrix size: {x1.Size}");
                    }
                    catch (Exception e)
                    {
                        
                    }
                    
                }
            },
            {
                "/check", (x) =>
                {
                    var index = Convert.ToInt32(x[1]);
                    Console.WriteLine(Equations[index].SolveVector);
                }
            },
            {
                "/invert", (x) =>
                {
                    try
                    {
                        string a = x[1];
                        Equation equation = Equations[Convert.ToInt32(a)];
                        // Console.WriteLine(NMath.NMath.MatrixOperation.MtrxProduct.Invert(equation.Nums));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }, {
                "/clear", (x) =>
                {
                    try
                    {
                        Equations.Clear();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            },
            {
                "/help", (x) =>
                {
                    try
                    {
                        Console.WriteLine($"\t/create n - create a new n-size matrix\n" +
                                          $"\t/solve key method - solve key equation with method method:\n" +
                                          $"\t\t-g - gauss\n" +
                                          $"\t\t-z - gauss-zeidel\n" +
                                          $"\t\t-p - BIG\n" +
                                          $"\t/read path - read a .txt file with matrix\n" +
                                          $"\t/check key - check solve vector of keys equation\n" +
                                          $"\t/show - check a list of equations and theirs keys\n");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        throw;    
                    }
                }
            }
            
        };

        static Matrix getTriangle(Matrix matrix)
        {
            Matrix ret = new Matrix(matrix.Row,matrix.Column);
            for (int i = 0; i < ret.Row; i++)
            {
                for (int j = i; j < ret.Column; j++)
                {
                    ret[i, j] = matrix[i, j];
                }
            }
            return ret;
        }
        static void Main(string[] args)
        {
            do
            {
                try
                {
                    // Matrix mt = new Matrix(new double[,]{{3,1,2,1},{0,8,3,2},{0,0,6,4},{0,0,0,4}});
                    // Matrix mt = new Matrix(new double[,]{{1, 3, 5}, {0, 1, 6}, {0, 0, 1}});
                    // Console.WriteLine(mt*NMath.NMath.MatrixOperation.TriangleInverse(mt));
                    // Matrix mt1 = new Matrix(new double[,]{{1, 0, 0, 0}, {2, 1, 0, 0}, {3, 5, 1, 0}, {8,3,1,1}});
                    // Console.WriteLine(NMath.NMath.MatrixOperation.TriangleInverse(mt1));
                    string[] jet = Console.ReadLine()?.Split(new char[] {' '});
                    actions[jet[0]](jet);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            } while (true);
            
        }
    }
}
