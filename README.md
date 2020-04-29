用户可使用现有的XuguDemo，调试运行，注意Ip地址填写
也可以根据 核心cs文件Program_CSharpDeamon.cs 自行构建项目工程
注意 目标框架为.NET Framework 4.5
驱动当前版本3.1
使用示例：
(1) 引用XuguClient.dll动态库文件;
(2) 将XGCSQL.dll和XuguClient.XML文件放于生成的应用程序的同级目录下；

demo主要演示了XuGu C#驱动操作数据库的以下几点功能：
(1) 构建连接（连接单机和连接集群）；
(2) 执行DDL、不带参数的DML语句；
(3) 执行带参数的SQL语句，包含了CSHARP绑定参数的几种方式；
(4) 通过SELECT语句获取结果集；
(5) 执行存储过程或存储函数，参数输入输出类型包括input、output、inputoutput类型；
(6) 执行存储过程或存储函数，并获取其中产生的结果集；
(7) 大对象的导入导出；
(8) 批量插入；
(9) 事务管理；

版本不一致可能导致动态库的导入不兼容，有问题请与xugu 的官方联系。
