db = db.getSiblingDB("ArgbControl");

db.createCollection("Clients");
db.createCollection("Sockets");

db.Clients.insertMany([
  {_id:ObjectId('6677475e6eb93ed64c597194'),Id:'6GE7BnevZEqDnnc7hcz2bA',Name:'flutter-app',Roles:['sender'],SecretHash:'Icus3JYzHXQqa4gyKAFS/+mADraw4x4C4YGHFeWrpf4=:MwqnxKMOddfH4obuJ5B/1WnmTMUBYOHz15D/lRbnhTE='},
  {_id:ObjectId('6677475e6eb93ed64c597195'),Id:'u8lf0zhHv0aEpHRbPMaAiA',Name:'esp32-livingroom',Roles:['receiver'],SecretHash:'Icus3JYzHXQqa4gyKAFS/+mADraw4x4C4YGHFeWrpf4=:MwqnxKMOddfH4obuJ5B/1WnmTMUBYOHz15D/lRbnhTE='}
]);

db.Sockets.insertOne({_id:ObjectId('6677471d6eb93ed64c597193'),Id:'FLpRNlMIoEqnlDrpWjDXxg',Name:'livingroom-leds',Clients:['u8lf0zhHv0aEpHRbPMaAiA','6GE7BnevZEqDnnc7hcz2bA'],OwnerId:'1'});
