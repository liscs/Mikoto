<h1 align="center">
  MisakaTranslator 女生自用版
  <br>
</h1>

![image](https://github.com/liscs/MisakaTranslator/assets/70057922/4e61a5c4-ec1f-402b-aa0c-e73c532c71f0)

<p align="center">
  <b>Galgame/文字游戏/漫画多语种实时机翻工具</b>
  <br>
  <b>开源 | 高效 | 易用</b>
  <br>
  <img src="https://github.com/liscs/MisakaTranslator/workflows/CI/badge.svg" alt="CI">
  <br>
  <br>
</p>

原始项目页：https://github.com/hanmin0822/MisakaTranslator

Textractor：https://github.com/Artikash/Textractor

Locale-Emulator：https://github.com/xupefei/Locale-Emulator

Macab分词词典：https://clrd.ninjal.ac.jp/unidic

本地翻译词典：https://freemdict.com


游戏配置文件示例
配置文件名：.\data\games\a3ff500d-5b56-4320-88e9-3452caa2d602.json
```json5
{
  "GameName": "アイコトバ-Silver Snow Sister-",//游戏名，默认使用所在文件夹名
  "FilePath": "F:\\ちゃろー！\\アイコトバ-Silver Snow Sister-\\AikotobaSSS.exe",//游戏启动路径
  "GameID": "a3ff500d-5b56-4320-88e9-3452caa2d602",
  "TransMode": 1,//1代表hook模式
  "SrcLang": "ja",
  "DstLang": "zh",
  "RepairFunc": "RepairFun_NoDeal",//文本预处理选项
  "RepairParamA": null,
  "RepairParamB": null,
  "HookCode": "HS65001#-3C@192A60",
  "HookCodeCustom": "HS65001#-3C@192A60:AikotobaSSS.exe",
  "Isx64": true,
  "MisakaHookCode": "【140192A60:1400CC555:0】",
  "Cleared": false
}
```
