# coding=utf-8
# 不加上面那行就不能写中文注释
# Iron Python自带了re，如果要使用其他标准库，可以把Python lib目录下的文件复制到翻译器的lib目录下。
import re

# 函数接受一个string，返回一个string，其他随意
def process(source):
    #去除HTML标签
    return re.sub(r"\s+", "", source)

