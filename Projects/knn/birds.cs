using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace BirdRecognition{
    class Program{
        public class BirdSample{
            public int label = -1;
            public List<double> features;
            public double[] sums;
            public double[] tSums;

            public BirdSample(){
                features = new List<double>();
                sums = new double[100];
                tSums = new double[100];
            }
            public BirdSample(int l, List<double> f){
                features = f;
                label = l;
            }
            public void calculateSums(){
                //For Each Feature.
                double percent = features.Count/100.0;
                for(int i = 0;i<features.Count;i++){
                    sums[(int)Math.Floor(i/percent)]+=Math.Abs(features[i]);
                }
            }

            public void calculateTSums(){
                for(int i = 1;i<features.Count;i++){
                    int sumIndex = (int)Math.Floor((double)i/features.Count*100);
                    int pSumIndex = (int)Math.Floor((double)i-1/features.Count*100);
                    int nSumIndex = (int)Math.Floor((double)i+1/features.Count*100);

                    if(sumIndex!=pSumIndex||sumIndex!=nSumIndex){
                        tSums[sumIndex]+=features[i];
                    }else{
                        tSums[sumIndex]+=2*features[i];
                    }
                }
                for(int i = 0;i<tSums.Length;i++){
                    tSums[i] = (int)Math.Floor(features.Count/100.0)/2.0*tSums[i];
                }
            }

            public void normalizeFeatures(){
                //Find Max and Min
                double max = 0;
                double min = 0;
                for(int i =0;i<features.Count;i++){
                    if(features[i]<min){
                        min = features[i];
                    }
                    if(features[i]>max){
                        max = features[i];
                    }
                }
                //Apply Formula
                for(int i =0;i<features.Count;i++){
                    double x = features[i];
                    features[i] = 2*((x-min)/(max-min))-1;
                }
            }

            public void normalizeZeroToOne(){
                //Find Max and Min
                double max = 0;
                double min = 0;
                for(int i =0;i<features.Count;i++){
                    if(features[i]<min){
                        min = features[i];
                    }
                    if(features[i]>max){
                        max = features[i];
                    }
                }
                //Apply Formula
                for(int i =0;i<features.Count;i++){
                    double x = features[i];
                    features[i] = ((x-min)/(max-min));
                }
            }

            public void isolatePeaks(double threshold){
                //Remove anything below a certain value.
                for(int i = 0;i<features.Count;i++){
                    if(Math.Abs(features[i])<threshold){
                        features[i]=0;
                    }else{
                        if(features[i]<0){
                            features[i]+=threshold;
                        }else{
                            features[i]-=threshold;
                        }
                    }
                }
                features.RemoveAll(i => i == 0);

                //Square samples to exaggerate difference.
                for(int i = 0;i<features.Count;i++){
                    int sign = (features[i]<0) ? -1 : 1;
                    //features[i] = Math.Pow(features[i],1.1);
                    features[i]*=sign;
                }
                //features.RemoveAll(i => i < 0);
            }
        }

        public static int ASCIIToDecimal(sbyte num1,sbyte num2)
        {
            int output = Convert.ToInt32(num1)+Convert.ToInt32(num2)*10;
            return output;
        }

        public static List<double> convertToDecimal(string filename){
            sbyte[] bytes = (sbyte[]) (Array)File.ReadAllBytes(filename);
            List<double> samples = new List<double>();
            for(int i = 44;i<bytes.Length-1;i+=2){
                int sample = ASCIIToDecimal(bytes[i],bytes[i+1]);
                samples.Add((double)sample*4);
            }
            return samples;
        }

        public static string readFile(string filename){
            string[] lines = File.ReadAllLines(filename);
            string data = "";
            for(int i = 0;i<lines.Length;i++){
                data+=lines[i];
            }
            return data;
        }

        public static BirdSample createSampleFromFile(int label,string filename){
            BirdSample bs = new BirdSample();
            List<double> bytes = convertToDecimal(filename);
            bs.label = label;
            bs.features = bytes;
            //bs.normalizeFeatures();
            bs.isolatePeaks(0);
            //bs.normalizeZeroToOne();
            bs.calculateSums();
            bs.calculateTSums();
            return bs;
        }

        public static List<BirdSample> buildTrainingData(string path){
            List<BirdSample> trainingData = new List<BirdSample>();
            trainingData.Add(createSampleFromFile(1,path+"mal1_song_1.wav"));
            trainingData.Add(createSampleFromFile(1,path+"mal1_song_3.wav"));

            trainingData.Add(createSampleFromFile(2,path+"mal2_song_1.wav"));
            trainingData.Add(createSampleFromFile(2,path+"mal2_song_2.wav"));

            trainingData.Add(createSampleFromFile(5,path+"mal5_song_1.wav"));
            trainingData.Add(createSampleFromFile(5,path+"mal5_song_2.wav"));

            trainingData.Add(createSampleFromFile(8,path+"mal8_song_5.wav"));
            trainingData.Add(createSampleFromFile(8,path+"mal8_song_8.wav"));

            trainingData.Add(createSampleFromFile(9,path+"mal9_song_2.wav"));
            trainingData.Add(createSampleFromFile(9,path+"mal9_song_4.wav"));

            trainingData.Add(createSampleFromFile(11,path+"mal11_song_5.wav"));
            trainingData.Add(createSampleFromFile(11,path+"mal11_song_6.wav"));

            return trainingData;
        }

        public static List<BirdSample> buildTestingData(string path){
            List<BirdSample> testingData = new List<BirdSample>();
            testingData.Add(createSampleFromFile(1,path+"mal1_song_8.wav"));
            testingData.Add(createSampleFromFile(2,path+"mal2_song_3.wav"));
            testingData.Add(createSampleFromFile(5,path+"mal5_song_3.wav"));
            testingData.Add(createSampleFromFile(8,path+"mal8_song_9.wav"));
            testingData.Add(createSampleFromFile(9,path+"mal9_song_6.wav"));
            testingData.Add(createSampleFromFile(11,path+"mal11_song_9.wav"));
            return testingData;
        }

        public static double birdDist(BirdSample a, BirdSample b){
            double distance = 0;
            int limit = (a.features.Count < b.features.Count) ? a.features.Count : b.features.Count;
            for(int i = 0;i<limit;i++){
                distance+=Math.Abs(a.features[i]-b.features[i]);
            }
            //Average limit, if it's absolute the shortest sample will always be predicted.
            return distance/limit;
        }

        public static double sumDist(BirdSample a, BirdSample b){
            double distance = 0;
            for(int i = 0;i<100;i++){
                double d = a.sums[i]-b.sums[i];
                distance+=Math.Abs(d);
            }
            return distance;
        }

        public static double tSumDist(BirdSample a, BirdSample b){
            double distance = 0;
            for(int i = 0;i<100;i++){
                distance+=Math.Abs(a.tSums[i]-b.tSums[i]);
            }
            return distance;
        }

        public static double crossCorrelation(BirdSample a, BirdSample b){

            double similarity_a_b = 0;
            double distance = 0;
            int limit = (a.features.Count < b.features.Count) ? a.features.Count : b.features.Count;
            for(int i = 0;i<limit;i++){

                //distance += Math.Pow(a.features[i], 2) - a.features[i]*b.features[i];
                similarity_a_b= checked(similarity_a_b + a.features[i]*b.features[i]);
            }
            //Console.WriteLine("mal5_song1 vs mal5_song2: {0}", similarity_a_b);
            
            return similarity_a_b;
        }

        public static Tuple<List<BirdSample>,List<BirdSample>> leaveOneOut(List<BirdSample> knownData){
            Random r = new Random();
            int index = r.Next(0,knownData.Count);
            List<BirdSample> trainingData = new List<BirdSample>();
            for(int i = 0;i<knownData.Count;i++){
                if(index!=i){
                    trainingData.Add(knownData[i]);
                }
            }
            List<BirdSample> testingData = new List<BirdSample>();
            testingData.Add(knownData[index]);
            Tuple<List<BirdSample>,List<BirdSample>> t = new Tuple<List<BirdSample>,List<BirdSample>>(trainingData,testingData);
            return t;
        }

        public static Tuple<List<BirdSample>,List<BirdSample>> holdout(List<BirdSample> knownData){
            List<BirdSample> trainingData = new List<BirdSample>();
            List<BirdSample> testingData = new List<BirdSample>();

            int[] usedIndeces = new int[knownData.Count];
            for(int i = 0;i<usedIndeces.Length;i++){
                usedIndeces[i] = 0;
            }
            Random r = new Random();

            int selectionSize = (int)((double)knownData.Count*0.2);
            while(testingData.Count<selectionSize){
                int x = r.Next(0,knownData.Count);
                if(usedIndeces[x]==0){
                    usedIndeces[x] = 1;
                    testingData.Add(knownData[x]);
                }
            }
            for(int i = 0;i<knownData.Count;i++){
                if(usedIndeces[i]==0){
                    trainingData.Add(knownData[i]);
                }
            }
            Tuple<List<BirdSample>,List<BirdSample>> t = new Tuple<List<BirdSample>,List<BirdSample>>(trainingData,testingData);
            //Console.WriteLine(t.Item1.Count+" "+t.Item2.Count);
            /*
            for(int i =0;i<t.Item1.Count;i++){
                Console.Write(t.Item1[i].label+" ");
            }
            Console.WriteLine();
            for(int i =0;i<t.Item2.Count;i++){
                Console.Write(t.Item2[i].label+" ");
            }
            Console.WriteLine();
            */
            return t;
        }

        public static double KNN(List<BirdSample> trainingData, List<BirdSample> testingData,int k){
            int estimates = testingData.Count;
            int correct = 0;
            //For each bird we want to find the label for.
            for(int i = 0;i<testingData.Count;i++){
                List<Tuple<BirdSample,double>> distances = new List<Tuple<BirdSample,double>>();
                //For each labelled bird.
                for(int j = 0;j<trainingData.Count;j++){
                    //Differences can be big, better not initialize it to some dummy value.
                    //double currentDist = KLDivergence(testingData[i],trainingData[j]);
                    double currentDist = birdDist(testingData[i],trainingData[j]);
                    //double currentDist = crossCorrelation(testingData[i],trainingData[j]);
                    //double currentDist = sumDist(testingData[i],trainingData[j]);
                    //double currentDist = tSumDist(testingData[i],trainingData[j]);
                    distances.Add(new Tuple<BirdSample,double>(trainingData[j],currentDist));
                }
                //Get count of each label in KNN
                List<Tuple<BirdSample,double>> sortedDistances = distances.OrderBy(o=>o.Item2).ToList();
                int[] labelCounts = {0,0,0,0,0,0,0,0,0,0,0,0};
                for(int ki = 0;ki<k;ki++){
                    labelCounts[sortedDistances[ki].Item1.label]++;
                }
                //Get label with most occurences.
                int bestLabel = 0;
                int maxOccurences = 0;
                for(int l = 0;l<labelCounts.Length;l++){
                    //Console.Write(labelCounts[l]);
                    if(labelCounts[l]>maxOccurences){
                        bestLabel = l;
                        maxOccurences = labelCounts[l];
                    }
                }
                //Console.WriteLine();
                //Console.WriteLine("P:{0} A:{1} D:{2}",label,testingData[i].label,minDist);
                if(testingData[i].label == bestLabel){
                    correct++;
                }
            }
            return ((double)correct/estimates);
        }

        public static int getIdealK(List<BirdSample> known){
            int runs = 1000;
            int maxK = known.Count-1;
            List<double> accuracies = new List<double>();
            for(int i = 0;i<maxK;i++){
                accuracies.Add(0);
            }
            for(int i = 0;i<runs;i++){
                for(int k = 1;k<maxK;k++){
                    Tuple<List<BirdSample>,List<BirdSample>> trainingAndTesting = leaveOneOut(known);
                    //Tuple<List<BirdSample>,List<BirdSample>> trainingAndTesting = holdout(known);
                    //testingData.Add(trainingAndTesting.Item2);
                    for(int t = 0;t<trainingAndTesting.Item2.Count;t++){
                        double accuracy = KNN(trainingAndTesting.Item1,trainingAndTesting.Item2,k);
                        accuracies[k]+=accuracy/trainingAndTesting.Item2.Count;
                    }
                }
            }
            double max = 0;
            int maxIndex = 1;
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

        public static double KLDivergence(BirdSample p, BirdSample q){
            double divergence = 0;
            int limit = (p.features.Count<q.features.Count) ? p.features.Count : q.features.Count;
            for(int i =0;i<limit;i++){
                divergence+=p.features[i]*Math.Log(p.features[i]/q.features[i],2);
            }
            return divergence;
        }

        public static void Main(){
            string path = "data/allSamples/";
            List<BirdSample> trainingData = buildTrainingData(path);
            List<BirdSample> testingData = buildTestingData(path);
            
            BirdSample b = createSampleFromFile(1,path+"mal1_song_8.wav");
            using (StreamWriter writer = new StreamWriter("out.csv"))  
            {  
                for(int i = 0;i<b.features.Count;i++){
                    writer.Write(b.features[i]);
                    if(i<b.features.Count-1){
                        writer.WriteLine(",");
                    }
                } 
            }

            int k = getIdealK(trainingData);
            Console.WriteLine("1NN: "+KNN(trainingData,testingData,1)*100+"% Accuracy");
            Console.WriteLine(k+"NN: "+KNN(trainingData,testingData,k)*100+"% Accuracy");
        }
    }
}
