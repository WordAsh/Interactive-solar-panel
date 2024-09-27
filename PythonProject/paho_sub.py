import paho.mqtt.client as mqtt

broker = "127.0.0.1"
port = 1883
keepalive = 60
topic = "rod_length/msg"

def on_connect(client,userdata,flags,rc):
    #客户端收到服务器的CONNACK相应时的回调
    print("Connected with result code " + str(rc))
    client.subscribe(topic)

def on_message(client,userdata,msg):
    #当从服务器收到PUBLISH消息时的回调
    print(msg.topic + " " + str(msg.payload))

client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message
client.connect(broker,port,keepalive)

client.loop_forever()