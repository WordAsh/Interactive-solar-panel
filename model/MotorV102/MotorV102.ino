/**********************************************************************
//本程序用来作为客户端接收数据控制两路步进电机动作

驱动器接线
ENA- -->GND
ENA+ -->GND，低或悬空有效，电机可以动作
DIR- -->GND
DIR+ -->22，这个是正反转控制的，根据需要接5V或GND控制正反转，也可以不接悬空
PUL- -->GND
PUL+ -->23

B-  -->蓝色
B+  -->红色
A-  -->绿色
A+  -->黑色
GND -->外部电源-
VCC -->外部电源+
**********************************************************************/
#include "main.h"

#define  VERSION   "V101"


void connect_wifi()                             //联网
{
  WiFi.mode(WIFI_STA);
  WiFi.begin();
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);         //用固定的账号密码连接网络
  
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print(".");
    delay(1000);
  }

  Serial.print("\nWiFi connected to: ");
  Serial.println(WIFI_SSID);
  Serial.print("IP:   ");
  Serial.println(WiFi.localIP());               //得到IP地址

  delay(500);                                   //延时1S
}

void mqtt_check_connect(void) {                  
  if (!mqttClient.connected()) {                //如果MQTT没有连接上的话
    Mqtt_State = false;
    Serial.print(F("MQTT state: "));
    Serial.println(mqttClient.state());
    
    String clientId = "ESP_53ec87c6";  
    if (mqttClient.connect(clientId.c_str())) { //连接服务器，连接成功的话订阅主题
      Serial.println("connect success!");
      Mqtt_State = true;                        //已经连接上

      mqttClient.subscribe("rod_length/msg");   //连接成功后订阅主题
    }
  }
}

void callback(char* topic, byte* payload, unsigned int length) {//mqtt回调函数，收到数据后调用此函数处理
  Serial.print("Message arrived [");            //打印主题
  Serial.print(topic);
  Serial.print("] ");

  String recv_data = "";
  for (int i = 0; i < length; i++) {            //打印接收到的数据，JSON数据
    //Serial.print((char)payload[i]);
    recv_data += (char)payload[i];
  }
  //Serial.println();

  Serial.print("recv data：");
  Serial.println(recv_data);
  recv_deal (recv_data);                        //对接收的数据进行处理
}

String String_Operation(String num1 ,String num2 ,String star)//获取特定区间的字符串
{
  int num3 = num1.length();                     //获取num1长度
  if(star.indexOf(num1) == -1 || star.indexOf(num2) == -1){//判断操作字符是否存在
    return "err";
  }
  else{                                         //寻找并截取特定区间的起始字符
    String date = star.substring(star.indexOf(num1)+num3,star.indexOf(num2));
    return date;
  }
}

void recv_deal(String recv)                     //处理接收到的数据
{
  float num1, num2;
  String str1 = String_Operation("(",",",recv);//解析数据
  String str2 = String_Operation(",",")",recv);//解析数据
  num1 = str1.toFloat();
  num2 = str2.toFloat();
  Serial.printf("num1：%.1f, num2：%.1f\r\n", num1, num2);
  Moto1_Num = num1 * (float)STEPS1_PER_MM;
  Moto2_Num = num2 * (float)STEPS2_PER_MM;

  
  Serial.printf("Moto1_Num：%d, Moto2_Num：%d\r\n", Moto1_Num, Moto2_Num);
}

void IRAM_ATTR onTimer()                        //定时中断函数
{
  if (Sys_State == 1){                          //回零状态
    if ((digitalRead(MIN1) == LOW) && (digitalRead(MIN2) == LOW)){//电机都回到零点了
      Sys_State = 0;
      Moto1_Count = 0;
      Moto2_Count = 0;
    }
    if (digitalRead(MIN1) == HIGH){             //电机1还没到零点
      digitalWrite(DIR1, BACK);
      if (Pul1_State == true){
        Pul1_State = false;
        digitalWrite(PUL1, HIGH);
      }
      else{
        Pul1_State = true;
        digitalWrite(PUL1, LOW);
      }
    }
    if (digitalRead(MIN2) == HIGH){             //电机2还没到零点
      digitalWrite(DIR2, BACK);
      if (Pul2_State == true){
        Pul2_State = false;
        digitalWrite(PUL2, HIGH);
      }
      else{
        Pul2_State = true;
        digitalWrite(PUL2, LOW);
      }
    }
  }
  else if (Sys_State == 0){                     //正常工作状态
    if (Moto1_Count != Moto1_Num){              //转动步数没有到
      Motor1_Dir = 0;
      if (Moto1_Count < Moto1_Num){             //伸出过程
        if (digitalRead(MAX1) == HIGH){         //没有到最大
          digitalWrite(DIR1, FORE);
          Motor1_Dir = 1;
        }
      }
      else if (Moto1_Count > Moto1_Num){        //收回过程
        if (digitalRead(MIN1) == HIGH){         //没有到最小
          digitalWrite(DIR1, BACK);
          Motor1_Dir = 2;
        }
      }
      
      if (Motor1_Dir > 0){                      //转动处理
        if (Pul1_State == true){
          Pul1_State = false;
          digitalWrite(PUL1, HIGH);
        }
        else{
          Pul1_State = true;
          digitalWrite(PUL1, LOW);
          if (Motor1_Dir == 1){
            Moto1_Count ++;
          }
          else{
            if (Moto1_Count > 0){
              Moto1_Count --;
            }
          }
        }
      }
    }
  
    if (Moto2_Count != Moto2_Num){              //转动步数没有到
      Motor2_Dir = 0;
      if (Moto2_Count < Moto2_Num){             //伸出过程
        if (digitalRead(MAX2) == HIGH){         //没有到最大
          digitalWrite(DIR2, FORE);
          Motor2_Dir = 1;
        }
      }
      else{                                     //收回过程
        if (digitalRead(MIN2) == HIGH){         //没有到最小
          digitalWrite(DIR2, BACK);
          Motor2_Dir = 2;
        }
      }
      
      if (Motor2_Dir > 0){                      //转动处理
        if (Pul2_State == true){
          Pul2_State = false;
          digitalWrite(PUL2, HIGH);
        }
        else{
          Pul2_State = true;
          digitalWrite(PUL2, LOW);
          if (Motor2_Dir == 1){
            Moto2_Count ++;
          }
          else{
            if (Moto2_Count > 0){
              Moto2_Count --;
            }
          }
        }
      }
    }
  }
}

void setup() 
{
  Serial.begin(9600);                         //初始化串口
  Serial.println();                             //打印回车换行

  pinMode(PUL1, OUTPUT);                        //初始化IO
  pinMode(DIR1, OUTPUT);
  pinMode(PUL2, OUTPUT);
  pinMode(DIR2, OUTPUT);
  pinMode(MAX1, INPUT_PULLUP);
  pinMode(MIN1, INPUT_PULLUP);
  pinMode(MAX2, INPUT_PULLUP);
  pinMode(MIN2, INPUT_PULLUP);

  Sys_State = 1;                                //回零状态
  connect_wifi();                               //联网处理

  mqttClient.setServer(mqttServer,mqttPort);    //MQTT初始化。初始化服务器
  mqttClient.setClient(espClient);              //初始化客户端
  mqttClient.setCallback(callback);             //设置回调函数
  
  timer0 = timerBegin(0, 80, true);             //初始化定时器
  timerAttachInterrupt(timer0, &onTimer, true); //设置定时中断回调函数 
  timerAlarmWrite(timer0, 20, true);           //设置定时时间单位us
  timerAlarmEnable(timer0);                     //启动定时器
}

void loop() 
{
  if (!mqttClient.connected()) {                //如果没有连接服务器的话，连接处理
    mqtt_check_connect();
  }
  mqttClient.loop();
}
