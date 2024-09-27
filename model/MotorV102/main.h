#ifndef __MAIN_H__
#define __MAIN_H__


#include <WiFi.h>
#include <PubSubClient.h>
#include <string.h>


#define FORE  LOW                   //杠子伸出
#define BACK  HIGH                  //杠子收回
//const int PULS_PER_REV = 6400;      //电机旋转一周脉冲数
const int STEPS1_PER_MM = 9143;     //1mm对应脉冲数，伸缩杆1
const int STEPS2_PER_MM = 7111;     //1mm对应脉冲数，伸缩杆2
const int MAX_MM = 500;             //最大动作距离mm

int PUL1 = 23;                      //PUL电机1
int DIR1 = 22;                      //DIR
int PUL2 = 21;                      //PUL电机2
int DIR2 = 19;                      //DIR
int MAX1 = 18;                      //最大限位电机1
int MIN1 = 5;                       //最小限位
int MAX2 = 27;                      //最大限位电机2
int MIN2 = 26;                      //最小限位

bool Pul1_State = false;            //输出状态，有信号为true
uint8_t Motor1_Dir = 0;             //转动方向，0不转，1正转，2反转
bool Pul2_State = false;            //输出状态，有信号为true
uint8_t Motor2_Dir = 0;             //转动方向，0不转，1正转，2反转
uint32_t Moto1_Num = 0;             //电机1要动作的步数，目标值
uint32_t Moto1_Count = 0;           //电机1要动作的步数计数，当前值
uint32_t Moto2_Num = 0;             //电机2要动作的步数，目标值
uint32_t Moto2_Count = 0;           //电机2要动作的步数计数，当前值

const char* WIFI_SSID     = "Redmi K60";   //家里无线路由器的账号和密码
const char* WIFI_PASSWORD = "123456789";
const char* mqttServer = "192.168.137.177";//mqtt服务器
const uint16_t mqttPort = 1883;     //端口
bool Mqtt_State = false;            //连接状态
WiFiClient espClient;
PubSubClient mqttClient;

uint8_t Sys_State = 0;              //系统状态，0为正常工作状态，1为上电回零状态

hw_timer_t * timer0 = NULL;         //硬件定时器
uint32_t T0_Count0 = 0;

typedef struct{                     //接收数据结构体
  char Recv_Buf[200];
  uint8_t Recv_Count = 0;
}TCP_RECV;

TCP_RECV recv_msg;                  //接收数据定义

#endif
