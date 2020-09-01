<?php
//Load in the data from file
$file = fopen('DT_irisTraining.csv','r');
$features = array();
$labels = array();
while($row = fgetcsv($file,',')){
    $rowFeatures=array();
    for($i=0;$i<count($row)-1;$i++){
        array_push($rowFeatures,$row[$i]);
    }
    array_push($features,$rowFeatures);
    array_push($labels,$row[count($row)-1]);
}

fclose($file);


$file = fopen('DT_irisTesting.csv','r');
$testingFeatures = array();
$testingLabels = array();
while($row = fgetcsv($file,',')){
    $rowFeatures=array();
    for($i=0;$i<count($row);$i++){
        array_push($rowFeatures,$row[$i]);
    }
    array_push($testingLabels,"undefined");
    array_push($testingFeatures,$rowFeatures);
}
fclose($file);
?>
<!doctype html>
<html>
<head>
<style>
*{
    text-align:center;
}
svg{
    width:100%;
    height:100%;
    min-height:100%;
    position:absolute;
    top:0;
    left:0;
}
body{
    height:100%;
    width:100%;
    min-height:100%;
    overflow:none;
    white-space:nowrap;
    padding:0;
    margin:0; 
    position:relative;
}
table{
    margin:10px;
    border: 2px black solid;
    display:inline-table;
}
table tr td{
    padding:10px;
}
.key{
    position:fixed;
    width:150px;
    background-color:white;
    border:1px gray solid;
}
.keyIcon{
    height:15px;
    width:15px;
    display:inline-block; 
}
#icon1{
    background-color:firebrick;
}
#icon2{
    background-color:green;
}
#icon3{
    background-color:skyblue;
}
.keyText{
    display:inline-block;
}
</style>
</head>
<body>
<svg id="svg">
</svg>
<div class="key">
<div class="keyIcon" id="icon1"></div>
<p class="keyText">1</p>
<div class="keyIcon" id="icon2"></div>
<p class="keyText">2</p>
<div class="keyIcon" id="icon3"></div>
<p class="keyText">3</p>
</div>
<div id="tree"></div>
<script>
function Sample(features,label){
    this.features = features;
    this.label = label;
}
function Table(samples){
    //Our table's entropy.
    this.entropy = 0;
    //How many times parent tables have split
    this.depth = 0;
    //The data contained within our table
    this.samples = samples;
    //Our known label, for training data
    this.decision = null;
    //Subtables.
    this.children = [];
    //The table split from
    this.parentTable = null;
    //Used for building the table graphically using BFS
    this.discovered = false;
    //Column parent table split on
    this.splitFrom = undefined;
    //Value parent table split on
    this.splitValue = undefined;
    //DOM representation of this table
    this.DOM = null;
    //Estimation using final decision tree
    this.guess = null;
}
function getTotalEntropy(table){
    //get counts of labels
    var total = table.samples.length;
    var counts = {};
    for(var i = 0;i<table.samples.length;i++){
        if(counts[table.samples[i].label]==undefined){
            counts[table.samples[i].label] = 1;
        }else{
            counts[table.samples[i].label] += 1;
        }
    }
    var entropy  = 0.0;
    Object.entries(counts).forEach(entry => {
        let attribute = entry[0];
        let ct = entry[1];
        var p = ct/total;
        entropy+=p*Math.log2(p);
    });
    return -entropy;
}

function getSplitEntropy(table,col,threshold){
    var lt = [];
    var gt = [];
    for(var i = 0;i<table.samples.length;i++){
        var sample = table.samples[i];
        if(sample.features[col]<=threshold){
            lt.push(sample);
        }else{
            gt.push(sample);
        }
    }
    return (getTotalEntropy(new Table(lt))+getTotalEntropy(new Table(gt)));
}

function getMaxInformationGain(table){
    var result = {};
    result.col = 0;
    result.threshold = 0.0;
    result.gain = 0.0;
    var maxIG = 0;
    var entropy = getTotalEntropy(table);

    for(var i = 0;i<table.samples[0].features.length;i++){
        for(var progress = 0;progress<10.0;progress = parseFloat(progress.toFixed(2))+0.1){
            var informationGain = entropy - getSplitEntropy(table,i,progress);
            if(informationGain>maxIG){
                maxIG = informationGain;
                result.col = i;
                result.threshold = progress;
                result.gain = maxIG;
            }
        }
    }
    console.log(result.gain);
    return result;
}

function splitTable(table,col,threshold){
    var lt = [];
    var gt = [];
    for(var i = 0;i<table.samples.length;i++){
        var sample = table.samples[i];
        if(sample.features[col]<=threshold){
            lt.push(sample);
        }else{
            gt.push(sample);
        }
    }
    var tables = [];
    tables.push(new Table(lt));
    tables.push(new Table(gt));
    return tables;
}

function buildDecisionTree(table){
    var res = getMaxInformationGain(table);
    //If we cannot learn more, stop and make a decision.
    if(res.gain <=0){
        console.log("halting tree");
            if(table.children.length==0){
                var labels = {};
                for(var i =0;i<table.samples.length;i++){
                    if(labels[table.samples[i].label]==null){
                        labels[table.samples[i].label]=1;
                    }else{
                        labels[table.samples[i].label]+=1;
                    }
                }
                var max = 0;
                Object.entries(labels).forEach(label => {
                    if(label[1]>max){
                        max = label[1];
                        table.decision = label[0];
                    }
                });
            }
            return;
    }
    //If we can learn more, split the table and get the minimum entropy for each subtable.
    var subTables = splitTable(table,res.col,res.threshold);
    table.children = subTables;
    subTables.forEach((t)=>{
        t.parentTable = table;
        t.depth = table.depth+1;
        t.splitFrom = res.col;
        t.splitValue = res.threshold;
    });
    buildDecisionTree(subTables[0]);
    buildDecisionTree(subTables[1]);
}

function buildTestTree(trainingTable,testingTable){
    console.log(trainingTable);
    console.log(testingTable);

    if(trainingTable.children.length<=0){
        testingTable.decision = trainingTable.decision;
        
        for(var i = 0;i<testingTable.samples.length;i++){
            testingTable.samples[i].guess = trainingTable.decision;
        }
        return;
    }

    var testingSubtables = splitTable(testingTable,trainingTable.children[0].splitFrom,trainingTable.children[0].splitValue);
    testingTable.children = testingSubtables;
    testingSubtables.forEach((t)=>{
        t.parentTable = testingTable;
        t.depth = testingTable.depth+1;
        t.splitFrom = trainingTable.children[0].splitFrom;
        t.splitValue = trainingTable.children[0].splitValue;
    });
    buildTestTree(trainingTable.children[0],testingTable.children[0]);
    buildTestTree(trainingTable.children[1],testingTable.children[1]);
}

function drawTable(table){

if(table.samples.length==0){
    return;
}

var t = document.createElement("table");
var tr = document.createElement("tr");
var th = document.createElement("th");
th.colSpan = table.samples[0].features.length;
tr.appendChild(th);
t.appendChild(tr);

th.innerHTML = "Depth: "+table.depth;
if(table.splitFrom!=undefined){
    var operation = (table === table.parentTable.children[0])? " <= " : " > ";
    th.innerHTML+=" | If Col "+table.splitFrom+operation+table.splitValue;
}
/*
for(var i = 0;i<table.samples.length;i++){
    var tr = document.createElement("tr");
    for(var j = 0;j<table.samples[i].features.length;j++){
        var td = document.createElement("td");
        td.innerHTML = table.samples[i].features[j];
        tr.appendChild(td);
    }
    var td = document.createElement("td");
    td.innerHTML = table.samples[i].label;
    tr.appendChild(td);
    t.appendChild(tr);
}
*/
if(table.decision!=undefined){
    if(table.decision=="1"){
        t.style.backgroundColor="firebrick";
        t.style.color="white";
        t.style.border="none";
    }else if(table.decision=="2"){
        t.style.backgroundColor="green";
        t.style.color="white";
        t.style.border="none";
    }else if(table.decision=="3"){
        t.style.backgroundColor="skyblue";
        t.style.color="white";
        t.style.border="none";
    }
}
table.DOM = t;
document.body.appendChild(t);

//Draw connection from table to its parent.
//get distance to parent.
    if(table.parentTable!=null){
        var ourBounds = table.DOM.getBoundingClientRect();
        var parentBounds = table.parentTable.DOM.getBoundingClientRect();
        //console.log(ourBounds.top, ourBounds.right, ourBounds.bottom, ourBounds.left);
        var newLine = document.createElementNS('http://www.w3.org/2000/svg','line');
        //newLine.setAttribute('id','line2');
        
        newLine.setAttribute('x1',ourBounds.left);
        newLine.setAttribute('y1',ourBounds.top);
        newLine.setAttribute('x2',(parentBounds.left+parentBounds.width/2));
        newLine.setAttribute('y2',parentBounds.bottom);
        newLine.setAttribute("stroke", "black");
        newLine.setAttribute("stroke-width", "2");
        document.getElementById("svg").appendChild(newLine);
    }
}


function drawDecisionTree(table){
    var queue = [];
    table.discovered = true;
    queue.push(table);
    var currentMaxDepth = 0; 
    while(queue.length>0){
        var v = queue.shift();
        if(v.depth>currentMaxDepth){
            currentMaxDepth = v.depth;
            document.write("<br>");
        }
        drawTable(v);
        for(var i = 0;i<v.children.length;i++){
            var w = v.children[i];
            if(w.discovered == false){
                w.discovered = true;
                queue.push(w);
            }
        }
    }
    document.write("<br>");
}

<?php
//Load in php data to js variables.
echo "var samples = [";
for($i = 0;$i<count($features);$i++){
    echo "new Sample([";
    for($j=0;$j<count($features[$i]);$j++){
        echo "'".$features[$i][$j]."'";
        if($j<count($features[$i])-1){
            echo ",";
        }
    }
    echo "],";
    echo "'".$labels[$i]."'";
    echo ")";
    if($i<count($features)){
        echo ",";
    }
}
echo "];";

echo "var testingSamples = [";
for($i = 0;$i<count($testingFeatures);$i++){
    echo "new Sample([";
    for($j=0;$j<count($testingFeatures[$i]);$j++){
        echo "'".$testingFeatures[$i][$j]."'";
        if($j<count($testingFeatures[$i])-1){
            echo ",";
        }
    }
    echo "],";
    echo "".$testingLabels[$i]."";
    echo ")";
    if($i<count($testingFeatures)){
        echo ",";
    }
}
echo "];";
?>
var table = new Table(samples);
var testingTable = new Table(testingSamples);
//console.log(table);
//console.log(testingTable);
buildDecisionTree(table);
drawDecisionTree(table);


buildTestTree(table,testingTable);
drawDecisionTree(testingTable);



var output = "";
for(var i =0;i<testingTable.samples.length;i++){
    output+=testingTable.samples[i].guess;
}
document.addEventListener('DOMContentLoaded', function() {
    var httpRequest = new XMLHttpRequest();
    httpRequest.onreadystatechange = function() {
        if (this.readyState == 4 && this.status == 200) {
            console.log(this.responseText);
        }
    };
    httpRequest.open("POST","out.php",true);
    httpRequest.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    httpRequest.send("output="+output);
}, false);
</script>
</body>
</html>
