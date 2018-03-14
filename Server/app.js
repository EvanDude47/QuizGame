var io = require('socket.io')(process.envPort||3000);
var MongoClient = require('mongodb').MongoClient;
var url = "mongodb://localhost:27017/";

console.log("Server Started");

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
});