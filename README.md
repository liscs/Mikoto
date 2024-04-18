# MisakaTranslator Mikoto Version
![image](https://github.com/liscs/MisakaTranslator/workflows/CI/badge.svg)
![image](https://github.com/liscs/MisakaTranslator/assets/70057922/0bf60266-63e6-4247-9cbb-ebe2b2008749)

MisakaTranslator Mikoto Version 是一款基于 MisakaTranslator 的互动文字小说阅读工具。
## 相较于原项目的重大变更
* 默认使用黑色文字描边，以利于视力健康。
* 使用json文件而非数据库管理游戏相关数据，以便于修改游戏信息。
* 加入了Azure翻译以及TTS（文本转语音）支持。
* 加入了火山翻译、Amazon Translate支持。
* 添加了更多的设置选项。

## 游戏信息配置文件示例
配置文件名：.\data\games\a3ff500d-5b56-4320-88e9-3452caa2d602.json
```json5
{
  "GameName": "アイコトバ-Silver Snow Sister-",//游戏名，默认使用游戏所在文件夹名
  "FilePath": "F:\\ちゃろー！\\アイコトバ-Silver Snow Sister-\\AikotobaSSS.exe",//游戏启动路径
  "GameID": "a3ff500d-5b56-4320-88e9-3452caa2d602",//添加游戏时随机生成的GUID
  "TransMode": 1,//1代表hook模式
  "SrcLang": "ja",//源语言
  "DstLang": "zh",//翻译目标语言
  "RepairFunc": "RepairFun_NoDeal",//文本预处理选项
  "RepairParamA": null,//文本预处理选项参数
  "RepairParamB": null,//文本预处理选项参数
  "HookCode": "HS65001#-3C@192A60",//Textractor特殊码
  "HookCodeCustom": "HS65001#-3C@192A60:AikotobaSSS.exe",//Textractor特殊码
  "Isx64": true,//64位游戏标记
  "MisakaHookCode": "【140192A60:1400CC555:0】"
}
```
## 相关链接
[原始项目页](https://github.com/hanmin0822/MisakaTranslator)、[Textractor](https://github.com/Artikash/Textractor)、[Locale-Emulator](https://github.com/xupefei/Locale-Emulator)、[Macab分词词典](https://clrd.ninjal.ac.jp/unidic)、[本地翻译词典](https://freemdict.com)
