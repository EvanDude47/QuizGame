var http = require('http');
var path = require('path');
var express = require('express');
var io = require('socket.io')(process.envPort||5000);
var MongoClient = require('mongodb').MongoClient;
var url = "mongodb://localhost:27017/";

console.log("Server Started");

//Web functionality
var app = express();

app.set('views', path.resolve(__dirname, 'views'));
app.set('view engine', 'ejs');

app.get("/", function(req, res){
    MongoClient.connect(url, function(err, db){
        if(err) throw err;
        var dbNet = db.db("QuizGameData");

        dbNet.collection("QuestionData").find().toArray(function(err, results){
            db.close();
            res.render("index", {QuestionData:results});
        });
    });
});

app.get("/scores", function(req, res){
    MongoClient.connect(url, function(err, db){
        if(err) throw err;
        var dbNet = db.db("QuizGameData");

        dbNet.collection("ScoreData").find().toArray(function(err, results){
            db.close();
            res.render("scores", {ScoreData:results});
        });
    });
});

http.createServer(app).listen(3000, function(){
    console.log("Question Site Running");
});


//Game functionality
var dbObj;

MongoClient.connect(url, function(err, client){
	if(err) throw err;

	dbObj = client.db("QuizGameData");
});

io.on('connection', function(socket){

    socket.on('open', function(){
        
		console.log("Client Connected");
	});

    socket.on('get data', function(data){

        dbObj.collection("QuestionData").find({}).toArray(function(err, res){
            if(err) throw err;
            socket.emit('receive data', res[0]);
        });
    });
	
    socket.on('send data', function(data){
		
        console.log(JSON.stringify(data));

        dbObj.listCollections({name: "QuestionData"}).next(function(err, collInfo){
            if(collInfo)
            {
                dbObj.dropCollection("QuestionData", function(err, delOK){
                    if(err) throw err;
                    if(delOK) console.log("Collection Reset");
            
                    dbObj.collection("QuestionData").save(data, function(collectionErr, res){
			            if(collectionErr) throw err;
			            console.log("Data saved to MongoDB")
		            });
                });
            }
            else
            {
                dbObj.collection("QuestionData").save(data, function(collectionErr, res){
			        if(collectionErr) throw err;
			        console.log("Data saved to MongoDB")
		        });
            }
        });
	});

    socket.on('push score', function(data){
        
        dbObj.collection("ScoreData").save(data, function(err, res){
			if(err) throw err;
		    console.log("Score saved to MongoDB")
		});
    });
});