using System;
using System.Collections.Generic;
using System.Linq;

namespace Numbers{
    class Program{
        public class NumberMatrix{
            public int label = -1;
            public List<double> features;

            public NumberMatrix(){
                features = new List<double>();
            }
        }
        public static NumberMatrix buildMatrixFromFile(string filename){
            NumberMatrix n = new NumberMatrix();
            string[] s = System.IO.File.ReadAllLines(filename);
            n.label = Int32.Parse(s[0][0].ToString());
            
            for(int i = 0;i < s.Length;i++){
                for(int j = 0;j<s[0].Length;j++){
                    if(i == 0 && j == 0){
                        continue;
                    }
                    if(i-1>=0){
                        if(s[i][j]=='1'&&s[i-1][j]=='0'){
                            char[] chars = s[i-1].ToCharArray();
                            chars[j] = '7';
                            s[i-1] = new string(chars);
                        }
                    }
                    if(j+1<s[i].Length){
                        if(s[i][j]=='1'&&s[i][j+1]=='0'){
                            char[] chars = s[i].ToCharArray();
                            chars[j+1] = '7';
                            s[i] = new string(chars);
                        }
                    }
                    if(i+1<s.Length){
                        if(s[i][j]=='1'&&s[i+1][j]=='0'){
                            char[] chars = s[i+1].ToCharArray();
                            chars[j] = '7';
                            s[i+1] = new string(chars);
                        }
                    }
                    if(j-1>=0){
                        if(s[i][j]=='1'&&s[i][j-1]=='0'){
                            char[] chars = s[i].ToCharArray();
                            chars[j-1] = '7';
                            s[i] = new string(chars);
                        }
                    }
                }
            }

            for(int i = 0;i < s.Length;i++){
                for(int j = 0;j<s[0].Length;j++){
                    if(i == 0 && j == 0){
                        continue;
                    }
                    if(i-1>=0){
                        if(s[i][j]=='7'&&s[i-1][j]=='0'){
                            char[] chars = s[i-1].ToCharArray();
                            chars[j] = '6';
                            s[i-1] = new string(chars);
                        }
                    }
                    if(j+1<s[i].Length){
                        if(s[i][j]=='7'&&s[i][j+1]=='0'){
                            char[] chars = s[i].ToCharArray();
                            chars[j+1] = '6';
                            s[i] = new string(chars);
                        }
                    }
                    if(i+1<s.Length){
                        if(s[i][j]=='7'&&s[i+1][j]=='0'){
                            char[] chars = s[i+1].ToCharArray();
                            chars[j] = '6';
                            s[i+1] = new string(chars);
                        }
                    }
                    if(j-1>=0){
                        if(s[i][j]=='7'&&s[i][j-1]=='0'){
                            char[] chars = s[i].ToCharArray();
                            chars[j-1] = '6';
                            s[i] = new string(chars);
                        }
                    }
                }
            }
                        
            for(int i = 0 ;i < s.Length;i++){
                Console.WriteLine(s[i]);
            }
            Console.WriteLine(); 
            for(int i =0;i < s.Length;i++){
                for(int j = 0;j<s[0].Length;j++){
                    if(i == 0 && j == 0){
                        continue;
                    }
                    if(s[i][j]=='7'){
                        n.features.Add(0.7);
                    }else if(s[i][j]=='6'){
                        n.features.Add(0.6);
                    }else{
                        n.features.Add(Int32.Parse(s[i][j].ToString()));
                    }
                }
            } 
            return n;
        }

        public static int getHammingDistance(NumberMatrix a, NumberMatrix b){
            int distance = 0;
            for(int i = 0;i<a.features.Count;i++){
                if(a.features[i]!=b.features[i]){
                    distance++;
                }
            }
            return distance;
        }

        public static double getMannhattanDistance(NumberMatrix a, NumberMatrix b){
            double distance = 0;
            for(int i = 0;i<a.features.Count;i++){
                distance+=Math.Abs(a.features[i]-b.features[i]);
            }
            return distance;
        }

        public static Tuple<List<NumberMatrix>,NumberMatrix> leaveOneOut(List<NumberMatrix> knownData){
            Random r = new Random();
            int index = r.Next(0,knownData.Count);
            List<NumberMatrix> trainingData = new List<NumberMatrix>();
            for(int i = 0;i<knownData.Count;i++){
                if(index!=i){
                    trainingData.Add(knownData[i]);
                }
            }
            Tuple<List<NumberMatrix>,NumberMatrix> t = new Tuple<List<NumberMatrix>,NumberMatrix>(trainingData,knownData[index]);
            return t;
        }

        public static string KNN(List<NumberMatrix> known, List<NumberMatrix> unknown, int k){
            string output="";
            for(int uk = 0;uk<unknown.Count;uk++){
                List<Tuple<NumberMatrix,double>> distances = new List<Tuple<NumberMatrix,double>>();
                for(int kn = 0;kn<known.Count;kn++){
                    //int distance = getHammingDistance(unknown[uk],known[kn]);
                    double distance = getMannhattanDistance(unknown[uk],known[kn]);
                    Tuple<NumberMatrix,double> dt = new Tuple<NumberMatrix,double>(known[kn],distance);
                    distances.Add(dt);
                }
                //Sort the distances
                List<Tuple<NumberMatrix,double>> sortedDistances = distances.OrderBy(o=>o.Item2).ToList();
                double[] labelCounts = {0,0,0,0,0,0,0,0,0,0};
                double weight = 1.0;
                for(int i = 0;i<k;i++){
                    labelCounts[sortedDistances[i].Item1.label]+=weight;
                    weight-=0.0001;
                }
                double max = -1;
                int maxIndex = 0;
                for(int i = 0;i<labelCounts.Length;i++){
                    if(labelCounts[i]>max){
                        max = labelCounts[i];
                        maxIndex = i;
                    }
                }
                output+=maxIndex.ToString();
            }
            return output;
        }

        public static List<NumberMatrix> getDataSet(bool testing){
            List<NumberMatrix> n  = new List<NumberMatrix>();
            if(testing){
            for(int i = 10;i<=39;i++){
                string filename = "nums/T"+i+".txt";
                n.Add(buildMatrixFromFile(filename));
            }
            return n;
            }
            for(int i = 0;i<=99;i++){
                string filename = "";
                if(i<10){
                    filename = "nums/0"+i+".txt";

                }else{
                    filename = "nums/"+i+".txt";
                }
                n.Add(buildMatrixFromFile(filename));
            }
            return n;
        }

        public static double checkAccuracy(string labels, List<NumberMatrix> actual){
            double numCorrect = 0;
            for(int i = 0 ;i<labels.Length;i++){
                if(labels[i].ToString()==actual[i].label.ToString()){
                    numCorrect++;
                }
            }
            return numCorrect/labels.Length;
        }

        public static int getIdealK(List<NumberMatrix> known){
            int runs = 1000;
            int maxK = known.Count-1;
            List<double> accuracies = new List<double>();
            for(int i = 0;i<maxK;i++){
                accuracies.Add(0);
            }
            for(int i = 0;i<runs;i++){
                for(int k = 1;k<maxK;k++){
                    List<NumberMatrix> testingData = new List<NumberMatrix>();
                    Tuple<List<NumberMatrix>,NumberMatrix> trainingAndTesting = leaveOneOut(known);
                    testingData.Add(trainingAndTesting.Item2);
                    string output = KNN(trainingAndTesting.Item1,testingData,k);
                    double accuracy = checkAccuracy(output,testingData);
                    accuracies[k]+=accuracy;
                }
            }
            double max = 0;
            int maxIndex = 0;
            for(int k = 1;k<accuracies.Count;k++){
                accuracies[k]/=runs;
                if(accuracies[k]>max){
                    maxIndex = k;
                    max = accuracies[k];
                }
                Console.WriteLine("K:{0} {1}%",k,accuracies[k]*100);
            }
            Console.WriteLine("Max K:{0}",maxIndex);
            return maxIndex;
        }

        public static void Main(){
            List<NumberMatrix> trainingData = getDataSet(false);
            List<NumberMatrix> testingData = getDataSet(true);
            
            int k = getIdealK(trainingData);
            string output = KNN(trainingData,testingData,k);
            for(int i = 0;i<testingData.Count;i++){
                Console.Write(testingData[i].label);
            }
            Console.WriteLine();
            Console.WriteLine(output);
            Console.WriteLine((checkAccuracy(output,testingData)*100)+"%");
        }
    }
}
