# cpdaily-auto-submit
今日校园表单自动提交，通用版。

![](https://github.com/cyanray/cpdaily-auto-submit/workflows/build/badge.svg)

## ⚠
我们学校已经不需要填报今日校园，因此本项目缺少测试。(可能需要各位网友帮助本项目的继续)

## 项目进度
- [x] 完成CpdailyCrypto类
- [x] 完成CpdailyCore类
- [x] 完善登录API
- [x] 完善表单提交API
- [x] 完成CLI程序
- [ ] 完成WebAPI程序

## 特性
- [ ] 通用的学工号登录(暂未完成，不推荐：会把今日校园挤下线)
- [ ] 手机验证码登录(暂未完成，不推荐：会把今日校园挤下线)
- [x] IDS登录
- [x] 表单获取与提交
- [x] 表单向导
- [x] 多用户支持
- [x] 填空表单项
- [x] 单选表单项
- [ ] 多选表单项 (缺少测试)
- [ ] 图片表单项 (缺少测试)

## 使用方法

### 0x00 准备运行环境

首先需要在[.Net Runtime 下载页](https://dotnet.microsoft.com/download/dotnet/current/runtime)下载并安装 **.NET5 Runtime** (提示：Run server apps下面的下载)。

然后在[Release页面](https://github.com/cyanray/cpdaily-auto-submit/releases)下载 cpdaily-auto-submit.zip，并解压到某个目录。

(你也可以在 [Actions](https://github.com/cyanray/cpdaily-auto-submit/actions) 中找到自动编译的测试版)

### 0x01 登录账号并完成表单向导
登录账号并执行表单向导。你需要根据向导的指示模拟完成一次表单，从而让程序学会自己填表单。

```bash
dotnet cpdaily-auto-submit.dll init -u "学号" -p "密码" -s "学校名称"
```

### 0x02 提交表单
对每一个账号，获取最新的未完成表单并根据配置提交表单。

```bash
dotnet cpdaily-auto-submit.dll submit
```
### 0x03 加入其他账号(仅限同一个学校)
加入新账号。

```bash
dotnet cpdaily-auto-submit.dll add-user -u "学号" -p "密码"
```

## 声明
一切开发旨在学习，请勿用于非法用途。
