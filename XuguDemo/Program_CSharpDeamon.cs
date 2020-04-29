using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using XuguClient;

using System.Drawing;
using System.Data;

using System.Data.Common;
using System.Collections;
using System.IO;
namespace TestCSharpDeamon
{
    class Program_CSharpDeamon
    {
        public static string conn_xg  = "IP=127.0.0.1;DB=DEMO;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK";

        static void Main(string[] args)
        {
            //环境初始化
            //======================================
            init_createtable();

            //IPS多节点连接示例
            //======================================
            test_ips_connect();

            //一般不带参数的 无需返回值的sql 的执行：  
            //======================================
            test_dmlCmdwithnoParams();

            //带参数的sql执行1 insert
            //==================================
            test_insertCmdwithParams();
            test_insertCmdwithParams2();

            //带参数的sql执行2 update  delete
            //======================================
            test_updateCmdwithParams();
            test_deleteCmdwithParams();
             
            //一般结果集的展示
            //======================================
            test_displayResult();

            //结果集的更新 同步至数据库
            //======================================
            test_ResultUpdate();

            //带参数的存储过程执行
            //======================================
            test_execprocedure();

            //带输出型参数的存储过程
            //======================================
            test_execprocedure_OutParam();

            ///函数执行
            //======================================
            test_execprocedure_fun();

            //存储过程/函数执行后提取其结果集
            //======================================
            test_execRefCursor();

            //大对象数据的录入与导出
            //======================================
            test_lob();

            //事务管理
            //======================================
            test_Transaction();

            //批量插入
            //======================================
            test_Cmd5();
            Console.ReadLine();
        }

        //环境初始化
        public static int  init_createtable()
        {
            XGConnection conn = new XGConnection();

            conn.ConnectionString = conn_xg ;
            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select count(*) from user_tables where table_name='TA'";// 
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    cmd.CommandText = "drop table TA cascade";
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = "select count(*) from user_tables where table_name='TP'";// 
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    cmd.CommandText = "drop table TP cascade";
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = "select count(*) from user_sequences where seq_name='SEQ_TP'";//T 
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    cmd.CommandText = "drop sequence SEQ_TP cascade";
                    cmd.ExecuteNonQuery();
                }
                cmd.CommandText = "select count(*) from user_sequences where seq_name='SEQ_TA'";// 
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    cmd.CommandText = "drop sequence SEQ_TA cascade";
                    cmd.ExecuteNonQuery();
                }

                string sql_str1 = "create table ta(id number,pid integer,p_float float,p_double double,pkey bigint,p_sint smallint,p_tint tinyint,name char(100),descri varchar(100),modify_time datetime default sysdate,p_numr numeric(4,2),p_clob clob,p_bool boolean,p_date date default sysdate,p_time time default sysdate,p_blob blob)";
                string sql_str2 = "create table tp(id number,pid integer,pname char(100),descri varchar(100),modify_time datetime default sysdate)";
                string sql_str3 = "create sequence seq_ta minvalue 1 maxvalue 999999999 start with 1 increment by 1 cache 20";
                string sql_str4 = "create sequence seq_tp minvalue 1 maxvalue 999999999 start with 1 increment by 1 cache 20";
                string sql_str5 = "create or replace view v_ap as  select ta.id as id1,tp.id id2,ta.name,tp.pname,ta.modify_time from tp left join ta on tp.pid=ta.pid";
                string sql_str6 = "create or replace trigger trig_identity_ta before insert on ta for each row begin if inserting and :new.id is null then  :new.id := seq_ta.nextval; end if;   end trig_identity_ta;";
                string sql_str7 = "create or replace trigger trig_identity_tp before insert on tp for each row begin if inserting and :new.id is null then  :new.id := seq_tp.nextval; end if;  end trig_identity_tp;";

                cmd.CommandText = sql_str1;
                cmd.ExecuteNonQuery();

                cmd.CommandText = sql_str2;
                cmd.ExecuteNonQuery();

                cmd.CommandText = sql_str3;
                cmd.ExecuteNonQuery();

                cmd.CommandText = sql_str4;
                cmd.ExecuteNonQuery();

                cmd.CommandText = sql_str5;
                cmd.ExecuteNonQuery();

                cmd.CommandText = sql_str6;
                cmd.ExecuteNonQuery();

                cmd.CommandText = sql_str7;
                cmd.ExecuteNonQuery();

                //使用完毕 释放资源 断开连接 
                cmd.Dispose();
                conn.Close();
                return 0;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                Console.WriteLine("测试 关闭连接后 连接当前状态" + conn.State.ToString());
                return -1;
            }
            
        }

        //IPS多节点连接示例
        public static int test_ips_connect()
        {
            string conn_ips = "IPS=192.168.1.1,127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK";
            XGConnection conn = new XGConnection();
            conn.ConnectionString = conn_ips;
            try
            {
                conn.Open();
                conn.Close();
                return 0;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                return -1;
            }
        }

        //执行无参数sql语句
        public static int test_dmlCmdwithnoParams()
        {
            XGConnection conn = new XGConnection();
            conn.ConnectionString = conn_xg;
            string insert_sql_ta = "insert into ta(pid,name) values(1,'张三')";
            string insert_sql_tp = "insert into tp(pid,pname,modify_time) values(1,'财务部',to_date('2016-04-21 12:12:00','yyyy-mm-dd hh:mi:ss'))";
            string sql_str = "update ta set descri='更新人员描述' where id=1";
            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;
                cmd.CommandText = insert_sql_ta;
                cmd.ExecuteNonQuery();

                cmd.CommandText = insert_sql_tp;
                cmd.ExecuteNonQuery();

                //====update         
                cmd.CommandText = sql_str;
                int effect_num = cmd.ExecuteNonQuery();

                //free cmd and disconnect 
                cmd.Dispose();
                conn.Close();
                return 0;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
        }

        //带参数的插入1
        public static int test_insertCmdwithParams()
        {

            XGConnection conn = new XGConnection();

            conn.ConnectionString = conn_xg;

            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;
                string sql = "insert into tp(pid,pname) values(?,?)";
             
                cmd.CommandText = sql;

                XGTransaction trans = conn.BeginTransaction();//为连接创建显示事务，此时该连接事务处于非自动提交状态
                cmd.Transaction = trans;//指定申明命令事务环境

                //参数赋值
                cmd.Parameters.Add("pid", XGDbType.Int).Value = 2;
                cmd.Parameters.Add("pname", XGDbType.VarChar).Value = "开发部";
                cmd.ExecuteNonQuery();

                //重复执行参数赋值前，需清除前次参数
                cmd.Parameters.Clear();

                //采用AddWithValue方法
                object t_int = 3;
                object t_str = "测试部";
                cmd.Parameters.AddWithValue("pid", t_int);
                cmd.Parameters.AddWithValue("pname", t_str);

                //采用AddRange方法
                cmd.Parameters.Clear();
                XGParameters[] t_Arr = new XGParameters[2];
                t_Arr[0] = new XGParameters("pid", XGDbType.Int);
                t_Arr[0].Value = 4;
                t_Arr[1] = new XGParameters("pname", XGDbType.VarChar);
                t_Arr[1].Value = "产品部";
                cmd.Parameters.AddRange(t_Arr);


                cmd.ExecuteNonQuery();

                trans.Commit();//提交命令事务

                //free cmd and disconnect 
                cmd.Dispose();
                conn.Close();
                return 0;

            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
        }
        
        //带参数的插入2
        public static int test_insertCmdwithParams2()
        {
            XGConnection conn = new XGConnection();
            conn.ConnectionString = conn_xg;
            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;
                string sql = "insert into ta(pid,name) values(?,?)";

                cmd.CommandText = sql;

                XGParameters pidParam = new XGParameters("pid", XGDbType.Int);
                pidParam.Direction = ParameterDirection.Input;
                pidParam.Value = 2;
                cmd.Parameters.Add(pidParam);

                string c1string = "李四";
                XGParameters nammeParam = new XGParameters("name", XGDbType.VarChar);
                nammeParam.Direction = ParameterDirection.Input;
                nammeParam.Value = c1string;
                cmd.Parameters.Add(nammeParam);
                 
                cmd.ExecuteNonQuery();

                //insert方法插入参数
                cmd.Parameters.Clear();
                cmd.Parameters.Insert(0,2);         //根据索引位置插入参数
                cmd.Parameters.Insert(1,"王五");
                cmd.ExecuteNonQuery();

                //重复执行参数赋值前，需清除前次参数
                cmd.Parameters.Clear();
                cmd.Parameters.Add("pid", XGDbType.Int).Value = 3;
                cmd.Parameters.Add("name", XGDbType.VarChar).Value = "赵七";
                cmd.Parameters.Add("remove", XGDbType.VarChar).Value = "待删除";
                //使用Remove方法删除指定位置的参数
                cmd.Parameters.Remove(2);
                //cmd.Parameters.Remove("remove");
                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();
                cmd.Parameters.Add("pid", XGDbType.Int).Value = 3;
                cmd.Parameters.Add("name", XGDbType.VarChar).Value = "钱八";
                cmd.ExecuteNonQuery();

                //free cmd and disconnect 
                cmd.Dispose();
                conn.Close();
                return 0;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
        }

        //带参数的 update
        public static int test_updateCmdwithParams()
        {
             XGConnection conn = new XGConnection();

            conn.ConnectionString = conn_xg;

            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;

                string sql_str = "update tp set descri=?,modify_time=?";
                cmd.CommandText = sql_str;

                cmd.Parameters.Add("descri", XGDbType.VarChar).Value = "测试数据变更";
                string strTime = "2017-12-12 12:11:11";
                DateTime dtTime = DateTime.Parse(strTime);
                cmd.Parameters.Add("modify_time", XGDbType.DateTime).Value = dtTime;
                int ret = cmd.ExecuteNonQuery();


                //free cmd and disconnect 
                cmd.Dispose();
                conn.Close();
                return ret;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
        }

        //带参数的 delete
        public static int test_deleteCmdwithParams()
        {
            XGConnection conn = new XGConnection();

            conn.ConnectionString = conn_xg;

            try
            {
                conn.Open();
                string sql_str = "delete from ta where pid=3";
                XGCommand cmd = new XGCommand(sql_str, conn);
                cmd.Connection = conn;

                Int32 ret = cmd.ExecuteNonQuery(); //return effect num 
                //free cmd and disconnect 
                cmd.Dispose();
                conn.Close();
                return ret;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
        }

        //一般结果集的展示
        public static int test_displayResult()
        {
            XGConnection conn = new XGConnection();
            conn.ConnectionString = conn_xg;
            DataSet ds = new DataSet();
            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;
                cmd.CommandText = "select * from ta;";
                XGDataAdapter t_Adapter = new XGDataAdapter(cmd);
                t_Adapter.Fill(ds, "ta");
                //=============================
                for (int i = 0; i < ds.Tables[0].Rows.Count;i++ )
                {
                    string val = "人员表数据: \n";
                    for (int j = 0; j < ds.Tables[0].Columns.Count;j++ )
                    {
                            val += ds.Tables[0].Rows[i][j].ToString()+"|";   //采用adapter类获取结果集数据
                    }
                }
                //=========== datareader 
                string sql_str2 = "select * from tp;";

                XGCommand cmd2 = conn.CreateCommand();// 或者 new DBCommand(sql_str, conn);
                cmd.CommandText = sql_str2;
                XGDataReader reader = cmd.ExecuteReader();
                XGDataRecord t_Record = new XGDataRecord(reader);
                Console.WriteLine("结果集列数：\t{0}", t_Record.FieldCount);
                while (reader.Read())
                {
                    XGDataRecord t_record = new XGDataRecord(reader);
                    string val = "人员数据: \n";
                    string id = reader.GetInt32(0).ToString();
                    string pid = reader.GetString(1);
                    string pname = Convert.ToString(t_record[2]);           //采用record类获取结果集
                    string desc = reader.IsDBNull(3) ? null : reader.GetString(3);
                    string date = reader.GetDateTime(4).ToString();

                    val += id + "|" + pid + "|" + pname + "|" + desc + "|" + date + "|";
                    Console.WriteLine(val);
                }

                reader.Dispose();
                cmd2.Dispose();
                cmd.Dispose();
                t_Adapter.Dispose();
                conn.Close();
                return 0;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }       
        }

        //结果集的更新 同步至数据库
        public static int test_ResultUpdate()
        {
            XGConnection conn = new XGConnection();
            conn.ConnectionString = conn_xg;

            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;
                string queryString = "select * from tp;";


                XGDataAdapter adapter = new XGDataAdapter();
                adapter.SelectCommand = new XGCommand(queryString, conn);

                string tableName = "tp";

                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet, tableName);

              
                foreach (DataRow dr in dataSet.Tables["tp"].Rows)
                {
                    if (dr["PNAME"].ToString().Trim().Equals("测试部"))
                    {
                        dr.Delete(); //删除DataSet 中的行
                        break;
                    }
                }
                dataSet.Tables[tableName].Rows[0][1] = 38;//更新DataSet中第一行第2列的值
                dataSet.Tables[tableName].Rows[1][3] = "开发项目";
                string[] dd = new String[5] { "124", "24", "dsf", "dsgrt", "2016-12-12 12:11:11" };
                dataSet.Tables[tableName].Rows.Add(dd);//增加一行 考虑参数的形式
                //增加一行 
                /*        string[] dd2 = new String[5] { "124", "13", "lhgdda", "第2列的值", "2016-05-13 09:15:11" };
                        dataSet.Tables[tableName].Rows.Add(dd2);//再增加一行 
                        */
                adapter.Update(dataSet, tableName);

                //
                dataSet.Dispose();
                adapter.Dispose();
                cmd.Dispose();
                conn.Close();
                return 0;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
        }

        //带参数的存储过程执行
        public static int test_execprocedure()
        {
            XGConnection conn = new XGConnection();

            conn.ConnectionString = conn_xg;

            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;

                cmd.CommandText = "select count(*) from user_tables where table_name='T_PROC10'";//T_pack_func1
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    cmd.CommandText = "drop table T_PROC10 cascade";
                    cmd.ExecuteNonQuery();
                }
                cmd.CommandText = "create table T_PROC10(pid int ,pname varchar)";
                cmd.ExecuteNonQuery();


               // string sql = "insert into tp(pid,pname) values(?,?)";
                string sql = "create or replace PROCEDURE P1(pid  INT,pname VARCHAR )";
                sql += " AS  BEGIN ";
                sql += " insert into T_PROC10 values(pid,pname);";
                sql += " end; ";

                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();


                cmd.CommandText = "P1";
                cmd.CommandType = CommandType.StoredProcedure;
           
                //参数赋值
                cmd.Parameters.Add("pid", XGDbType.Int).Value = 2;
                cmd.Parameters.Add("pname", XGDbType.VarChar).Value = "开发部";


                cmd.ExecuteNonQuery();
                //重复执行参数赋值前，需清除前次参数
                cmd.Parameters.Clear();
                cmd.Parameters.Add("pid", XGDbType.Int).Value = 3;
                cmd.Parameters.Add("pname", XGDbType.VarChar).Value = "测试部";
                cmd.ExecuteNonQuery();

                //free cmd and disconnect 
                cmd.Dispose();
                conn.Close();
                return 0;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
        }

        //存储过程/函数执行后提取其结果集
        public static int test_execRefCursor()
        {
            XGConnection conn = new XGConnection();
            conn.ConnectionString = conn_xg;
            XGDataReader t_Reader = new XGDataReader();
            XGParameters t_Param = new XGParameters("ARG", XGDbType.RefCursor);
            XGRefCursor t_Cur = new XGRefCursor();
            t_Param.Direction = System.Data.ParameterDirection.Output;
            string results = null;
            try 
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;

                //创建存储过程及相关表
                cmd.CommandText = "SELECT COUNT(*) FROM ALL_TABLES WHERE TABLE_NAME='T_REFCURSOR';";
                if (Convert.ToInt16(cmd.ExecuteScalar()) != 0)
                {
                    cmd.CommandText = "DROP TABLE T_REFCURSOR CASCADE ;";
                    cmd.ExecuteNonQuery();
                }
                cmd.CommandText = "CREATE TABLE T_REFCURSOR(COL INTEGER,COL2 VARCHAR);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO T_REFCURSOR VALUES(1,'xugu');";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE OR REPLACE PROCEDURE T_PROC_REF(ARG OUT SYS_REFCURSOR)AS BEGIN OPEN ARG FOR SELECT * FROM T_REFCURSOR; END;";//OUT TYPE 
                cmd.ExecuteNonQuery();

                //执行存储过程
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "T_PROC_REF";
                cmd.Parameters.Clear();
                cmd.Parameters.Add(t_Param);
                cmd.ExecuteNonQuery();
                t_Cur = ((XGRefCursor)t_Param.Value);
                t_Reader = t_Cur.GetDataReader();
                if (t_Reader.Read())
                {
                    string val = "RefCursor结果集展示: \n";
                    val += Convert.ToInt32(t_Reader.GetValue(0)).ToString() + '|';
                    val += Convert.ToString(t_Reader.GetValue(1));
                    Console.WriteLine(val);
                }
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
            return 0;
        }

        //带输出型参数的存储过程执行
        public static int test_execprocedure_OutParam()
        {
            XGConnection conn = new XGConnection();
            conn.ConnectionString = conn_xg; 
            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn; 

                cmd.CommandText = "select count(*) from user_tables where table_name='TEST_CSH_PROC'";
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    cmd.CommandText = "drop table TEST_CSH_PROC cascade";
                    cmd.ExecuteScalar();
                }
                cmd.CommandText = "CREATE TABLE TEST_CSH_PROC(ID INT,SS VARCHAR(20),KK DATE,SP NUMERIC(12,5),PP FLOAT,TT FLOAT)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into TEST_CSH_PROC values(1,'testss',TO_DATE('2016-1-1 23:45:09','YYYY-MM-DD HH24:MI:SS'),321.34,123.123,321.123)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into TEST_CSH_PROC values(1,'testss',TO_DATE('2016-1-1 23:45:09','YYYY-MM-DD HH24:MI:SS'),111.23,231.12,322)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into TEST_CSH_PROC values(1,'testss',TO_DATE('2016-1-1 23:45:09','YYYY-MM-DD HH24:MI:SS'),234.123,986.234,9322)";
                cmd.ExecuteNonQuery();


                string sql_str = "CREATE OR REPLACE PROCEDURE P1(IN_ID IN OUT INT,OSS OUT VARCHAR,OKK OUT DATE,OSP  OUT NUMERIC,OPP OUT FLOAT,OTT OUT FLOAT)";
                sql_str += "AS TEMP_ID INT; BEGIN";
                sql_str += " select count(*),ss,kk,sum(sp),sum(pp),sum(tt) into temp_id,oss,okk,osp,opp,ott from TEST_CSH_PROC where id=IN_ID group by ss,kk;";
                sql_str += " IN_ID:=TEMP_ID; end;";
                cmd.CommandText = sql_str;
                cmd.ExecuteNonQuery();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "P1";
                XGParameters par_id = new XGParameters("IN_ID", XGDbType.Int);
                par_id.Direction = ParameterDirection.InputOutput;
                par_id.Value = 1;
                cmd.Parameters.Add(par_id);
                XGParameters par_oss = new XGParameters("OSS", XGDbType.VarChar, 500);
                par_oss.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(par_oss);
                XGParameters par_okk = new XGParameters("OKK", XGDbType.DateTime);
                par_okk.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(par_okk);
                XGParameters par_osp = new XGParameters("OSP", XGDbType.Numeric);
                par_osp.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(par_osp);
                XGParameters par_opp = new XGParameters("OPP", XGDbType.Double);
                par_opp.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(par_opp);
                XGParameters par_ott = new XGParameters("OTT", XGDbType.Double);
                par_ott.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(par_ott);
                cmd.ExecuteNonQuery();

                Console.WriteLine("输出型参数返回值：");
                Console.WriteLine("in_id=" + par_id.Value.ToString());
                Console.WriteLine("oss=" + par_oss.Value.ToString());
                Console.WriteLine("okk=" + par_okk.Value.ToString());
                Console.WriteLine("osp=" + par_osp.Value.ToString());
                Console.WriteLine("opp=" + par_opp.Value.ToString());
                Console.WriteLine("ott=" + par_ott.Value.ToString());

                cmd.Dispose();
                conn.Dispose();
                return 0;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
        }

        //函数执行
        public static int test_execprocedure_fun()
        {
            XGConnection conn = new XGConnection();
            conn.ConnectionString = conn_xg;

            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;
 
                cmd.CommandText = "select count(*) from user_tables where table_name='T_PACK_FUNC1'";//T_pack_func1
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    cmd.CommandText = "drop table T_PACK_FUNC1 cascade";
                    cmd.ExecuteScalar();
                }

                cmd.CommandText = "create table T_pack_func1(c1 int ,c2 double,c3 datetime,c4 numeric(32,8),c5 varchar)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = " insert into  T_pack_func1 values(1,null,'2017-07-10 15:01:35',33493.23423,'this is the func1 values 1')";
                cmd.ExecuteNonQuery();
                cmd.CommandText = " insert into  T_pack_func1 values( 2,23423.23,null,98763.2333,'here is the func1 the No2 value and so we need ')";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into  T_pack_func1 values( 3,972.332,'2017-07-09 19:45:22',null,'so we get the third 3 value')";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "insert into  T_pack_func1 values( 4, 243.2342,'2017-07-04 20:35:18',3454.32,null)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into  T_pack_func1 values( 5,843.23,'2008-09-01 12:25:38',205.23,'')";
                cmd.ExecuteNonQuery();
                //
                cmd.CommandText = "select count(*) from user_tables where table_name='T_PACK_PAN1'";//T_pack_pan1
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    cmd.CommandText = "drop table T_PACK_PAN1 cascade";
                    cmd.ExecuteScalar();
                }
                cmd.CommandText = "create table T_pack_pan1(c1 bigint,c2 char(50),c3 date,c4 int, c5 float)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into T_pack_pan1 values(123,'sdishosho hereess ','2017-07-01',null,324.56)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into T_pack_pan1 values(456,'some get out the sdishosho hereess ',null,234,45.324) ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into T_pack_pan1 values(789,null,'2017-07-03',34,23445.23) ";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into T_pack_pan1 values(678,'the 4fourth in ','2017-07-04',44,null)";
                cmd.ExecuteNonQuery();

                // HEAD 
                cmd.CommandText = "select count(*) from user_PACKAGES where PACK_name='PACK_NAME1'";//PACK_NAME1
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                {
                    //  cmd.CommandText = "drop table T_PACK_PAN1 cascade";
                    // cmd.ExecuteScalar();
                    cmd.CommandText = "alter package pack_name1 recompile";
                    cmd.ExecuteScalar();
                }
                else
                {
                    string sql_pack_head = "create or replace  package pack_name1 is ";
                    sql_pack_head += " function pa_func1(aa in int,";
                    sql_pack_head += " bb in out double,";
                    sql_pack_head += " cc out datetime,";
                    sql_pack_head += " dd out numeric,";
                    sql_pack_head += " ee out varchar)";
                    sql_pack_head += " return datetime;";
                    sql_pack_head += " procedure proc_pan1";
                    sql_pack_head += "(aa in bigint,bb in out char(50), cc out date, dd out int, ee out float);";
                    sql_pack_head += " end; ";

                    cmd.CommandText = sql_pack_head;
                    cmd.ExecuteNonQuery();
                    // cmd.ExecuteScalar();
                    //body 
                    string sql_pack_body = "create or replace package body pack_name1 is";
                    sql_pack_body += " function pa_func1(";
                    sql_pack_body += " aa in int,  bb in out double,  cc out datetime,  dd out numeric,  ee out varchar  )";
                    sql_pack_body += " return datetime   as   TMP_DT DATETIME; begin";
                    sql_pack_body += " select c2 ,c3,c4,c5 into bb,cc,dd,ee from T_pack_func1 where c1=aa;";
                    sql_pack_body += " TMP_DT:=cc;"; //sql_pack_body += "";
                    sql_pack_body += " return TMP_DT;end;";
                    sql_pack_body += " procedure proc_pan1(";
                    sql_pack_body += " aa in bigint,     bb in out char(50),     cc out date,     dd out int,     ee out float)";
                    sql_pack_body += " as begin ";
                    sql_pack_body += " select c2 ,c3,c4,c5 into bb,cc,dd,ee from T_pack_pan1  where c1=aa;";
                    sql_pack_body += " end;  ";
                    sql_pack_body += " end; ";

                    cmd.CommandText = sql_pack_body;
                    cmd.ExecuteNonQuery();

                }
                //==func ====================
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "PACK_NAME1.PA_FUNC1";
                XGParameters aa = new XGParameters("AA", XGDbType.Int);
                aa.Direction = ParameterDirection.Input;
                aa.Value =3;//2 3 4 5
                cmd.Parameters.Add(aa);

                XGParameters bb = new XGParameters("BB", XGDbType.Double);
                bb.Direction = ParameterDirection.InputOutput;
                bb.Value = 1.0;
                cmd.Parameters.Add(bb);

                XGParameters cc = new XGParameters("CC", XGDbType.DateTime);
                cc.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(cc);

                XGParameters dd = new XGParameters("DD", XGDbType.Numeric);
                dd.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(dd);

                XGParameters ee = new XGParameters("ee", XGDbType.VarChar, 200);
                ee.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(ee);

                XGParameters ff = new XGParameters("ff", XGDbType.DateTime);
                ff.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(ff);
                // fun ================
                cmd.ExecuteNonQuery();

                Console.WriteLine("存储函数参数和返回值输出：");
                Console.WriteLine("aa=" + aa.Value.ToString());
                Console.WriteLine("bb=" + bb.Value.ToString());
                Console.WriteLine("cc=" + cc.Value.ToString());
                Console.WriteLine("dd=" + dd.Value.ToString());
                Console.WriteLine("ee=" + ee.Value.ToString());
                Console.WriteLine("ff=" + ff.Value.ToString());
                //=====================================================


                //free cmd and disconnect 
                cmd.Dispose();
                conn.Close();

                return 0;
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
        }

        //LOB文件路径
        public static string c_filepath = "..//..//t_clob.pdf";
        public static string b_filepath = "..//..//t_blob.mp3";
        public static string cp_filepath = "..//..//getClob.pdf";
        public static string bp_filepath = "..//..//getBlob.mp3";
      
        //blob  clob insert
        static byte[] buff = new byte[1024000];
        static Int32 Count;
        public static int test_lob()
        {
            XGConnection conn = new XGConnection();
            conn.ConnectionString = conn_xg;
            XGClob t_Clob = new XGClob();
            XGBlob t_Blob = new XGBlob();
            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT COUNT(*) FROM ALL_TABLES WHERE TABLE_NAME='TEST_LOB'";
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    cmd.CommandText = "CREATE TABLE TEST_LOB(COL INT,COL2 BLOB,COL3 CLOB)";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd.CommandText = "DROP TABLE TEST_LOB";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "CREATE TABLE TEST_LOB(COL INT,COL2 BLOB,COL3 CLOB)";
                    cmd.ExecuteNonQuery();
                }
                //装载BLOB和CLOB数据
                using (FileStream t_f = new FileStream(b_filepath, FileMode.Open, FileAccess.Read))
                {
                    Array.Clear(buff, 0, buff.Length);
                    Count = t_f.Read(buff, 0, buff.Length);
                    t_Blob.BeginChunkWrite();
                    while (Count > 0)
                    {
                        t_Blob.write(buff, 0, Count);
                        Array.Clear(buff, 0, buff.Length);
                        Count = t_f.Read(buff, 0, buff.Length);
                    }
                    t_Blob.EndChunkWrite();
                }
                using (FileStream t_f = new FileStream(c_filepath, FileMode.Open, FileAccess.Read))
                {
                    Array.Clear(buff, 0, buff.Length);
                    Count = t_f.Read(buff, 0, buff.Length);
                    t_Clob.BeginChunkWrite();
                    while (Count > 0)
                    {
                        t_Clob.write(buff, 0, Count);
                        Array.Clear(buff, 0, buff.Length);
                        Count = t_f.Read(buff, 0, buff.Length);
                    }
                    t_Clob.EndChunkWrite();
                }
                //插入大对象
                cmd.CommandText = "INSERT INTO TEST_LOB VALUES(1,?,?);";
                cmd.Parameters.Clear();
                cmd.Parameters.Add("COL2", XGDbType.LongVarBinary).Value = t_Blob;
                cmd.Parameters.Add("COL3", XGDbType.LongVarChar).Value = t_Clob;
                cmd.ExecuteNonQuery();
                t_Clob.Dispose();
                t_Blob.Dispose();  
           
                //导出大对象
                cmd.CommandText = "SELECT COL2,COL3 FROM TEST_LOB WHERE COL=1;";
                XGDataReader t_Reader = cmd.ExecuteReader();
                if (t_Reader.Read())
                {
                    t_Blob = t_Reader.GetXGBlob(0);
                    t_Clob = t_Reader.GetXGClob(1);  
                    //CLOB
                    using (FileStream t_f = new FileStream(cp_filepath, FileMode.Create, FileAccess.Write))
                    {
                        Array.Clear(buff, 0, buff.Length);
                        Console.WriteLine("CLOB导出文件大小：{0}",t_Clob.Length);
                        Count = t_Clob.Read(buff, 0, buff.Length);
                        while (Count > 0)
                        {
                            t_f.Write(buff, 0, Count);
                            Array.Clear(buff, 0, buff.Length);
                            Count = t_Clob.Read(buff, 0, buff.Length);
                        }
                    }
                    //BLOB
                    using (FileStream t_f = new FileStream(bp_filepath, FileMode.Create, FileAccess.Write))
                    {
                        Array.Clear(buff, 0, buff.Length);
                        Console.WriteLine("BLOB导出文件大小：{0}", t_Blob.Length);
                        Count = t_Blob.Read(buff, 0, buff.Length);
                        while (Count > 0)
                        {
                            t_f.Write(buff, 0, Count);
                            Array.Clear(buff, 0, buff.Length);
                            Count = t_Blob.Read(buff, 0, buff.Length);
                        }
                    }
                }
                t_Reader.Close();
                t_Clob.Dispose();
                t_Blob.Dispose();
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                return -1;
            }
            finally
            {
                conn.Close();
            }
            return 0;
        }

        //批量插入
        public static int rec_num = 10000;
        public static int array_num = 10000;
        public static Random rd = new Random();
        public static DateTime RandomDate(DateTime minDate, DateTime maxDate)
        {

            int totalsec = (int)((TimeSpan)maxDate.Subtract(minDate)).TotalSeconds;
            int randomDays = rd.Next(0, totalsec);
            return minDate.AddSeconds(randomDays);
        }
        public static string[] ret_dt_str(int array_len)
        {
            try
            {
                DateTime maxdate = Convert.ToDateTime("2017-1-1 00:00:00");
                DateTime mindate = Convert.ToDateTime("1990-1-1 00:00:00");
                string[] dt = new string[array_len];
                for (int i = 0; i < array_len; i++)
                {
                    dt[i] = RandomDate(mindate, maxdate).ToString("yyyy-MM-dd HH:mm:ss"); ;
                }
                return dt;
            }
            catch (Exception ei)
            {
                Console.WriteLine(ei.ToString());
                return null;
            }
        }
        public static DateTime[] ret_dt_val(int array_len)
        {
            try
            {
                DateTime maxdate = Convert.ToDateTime("2017-1-1 00:00:00");
                DateTime mindate = Convert.ToDateTime("1990-1-1 00:00:00");
                DateTime[] dt = new DateTime[array_len];
                for (int i = 0; i < array_len; i++)
                {
                    dt[i] = RandomDate(mindate, maxdate);
                }
                return dt;
            }
            catch (Exception ei)
            {
                Console.WriteLine(ei.ToString());
                return null;
            }
        }
        public static int test_Cmd5()
        {
            string sql_str = "CREATE TABLE TEST_DT(DT1 DATETIME,DT2 DATETIME,DT3 DATETIME)";
            XGConnection conn = new XGConnection();

            conn.ConnectionString = conn_xg;
            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;
                cmd.CommandText = "SELECT COUNT(*) FROM ALL_TABLES WHERE TABLE_NAME='TEST_DT'";
                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                {
                    cmd.CommandText = "DROP TABLE TEST_DT";
                    cmd.ExecuteNonQuery();
                }
                cmd.CommandText = sql_str;
                cmd.ExecuteNonQuery();//创建测试基础表结构
                cmd.CommandText = "INSERT INTO TEST_DT VALUES(?,?,?)";
                DateTime[] dt1 = new DateTime[array_num];
                DateTime[] dt2 = new DateTime[array_num];
                DateTime[] dt3 = new DateTime[array_num];
                dt1 = ret_dt_val(array_num);
                dt2 = ret_dt_val(array_num);
                dt3 = ret_dt_val(array_num);
                object[] objs = new object[3];
                objs[0] = dt1;
                objs[1] = dt2;
                objs[2] = dt3;
                //非批处理，时间类型传入参数为时间类型
                DateTime bt = DateTime.Now;
                for (int rec = 0; rec < rec_num; rec++)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("?", XGDbType.DateTime).Value = ((DateTime[])objs[0])[rec];
                    cmd.Parameters.Add("?", XGDbType.DateTime).Value = ((DateTime[])objs[1])[rec];
                    cmd.Parameters.Add("?", XGDbType.DateTime).Value = ((DateTime[])objs[2])[rec];
                    cmd.ExecuteNonQuery();
                }
                DateTime ed = DateTime.Now;
                TimeSpan ts = ed - bt;
                Console.WriteLine("非批处理插入3列datetime类型表传入时间类型参数" + rec_num + "条记录耗时：" + ts.ToString());

                string[] dt_str1 = new string[array_num];
                string[] dt_str2 = new string[array_num];
                string[] dt_str3 = new string[array_num];
                dt_str1 = ret_dt_str(array_num);
                dt_str2 = ret_dt_str(array_num);
                dt_str3 = ret_dt_str(array_num);
                object[] objs2 = new object[3];
                objs2[0] = dt_str1;
                objs2[1] = dt_str2;
                objs2[2] = dt_str3;
                bt = DateTime.Now;
                for (int rec = 0; rec < rec_num; rec++)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("?", XGDbType.VarChar).Value = ((string[])objs2[0])[rec];
                    cmd.Parameters.Add("?", XGDbType.VarChar).Value = ((string[])objs2[1])[rec];
                    cmd.Parameters.Add("?", XGDbType.VarChar).Value = ((string[])objs2[2])[rec];
                    cmd.ExecuteNonQuery();
                }
                ed = DateTime.Now;
                ts = ed - bt;
                Console.WriteLine("非批处理插入3列datetime类型表传入字符串类型参数" + rec_num + "条记录耗时：" + ts.ToString());
                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT COUNT(*) FROM TEST_DT WHERE 1=1";
                if (Convert.ToInt32(cmd.ExecuteScalar()) != rec_num * 2)
                {
                    Console.WriteLine("插入时间类型测试后，数据条数异常");
                }

                //批处理
                cmd.CommandText = "INSERT INTO TEST_DT VALUES(?,?,?)";

                cmd.ArrayBindCount = rec_num;
                for (int col_no = 0; col_no < 3; col_no++)
                {
                    XGParameters par = new XGParameters("?", XGDbType.VarChar);
                    par.Direction = ParameterDirection.Input;
                    par.Value = objs2[col_no];
                    cmd.Parameters.Add(par);
                }
                bt = DateTime.Now;
                cmd.ExecuteNonQuery();
                ed = DateTime.Now;
                ts = ed - bt;
                Console.WriteLine("批处理插入3列datetime类型表传入字符串类型参数" + rec_num + "条记录耗时：" + ts.ToString());

                cmd.Parameters.Clear();

                cmd.ArrayBindCount = rec_num;
                for (int col_no = 0; col_no < 3; col_no++)
                {
                    XGParameters par = new XGParameters("?", XGDbType.DateTime);
                    par.Direction = ParameterDirection.Input;
                    par.Value = objs[col_no];
                    cmd.Parameters.Add(par);
                }
                bt = DateTime.Now;
                cmd.ExecuteNonQuery();
                ed = DateTime.Now;
                ts = ed - bt;
                Console.WriteLine("批处理插入3列datetime类型表传入时间类型参数" + rec_num + "条记录耗时：" + ts.ToString());
                cmd.Dispose();
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                return -1;
            }
            finally
            {
                conn.Close();
                Console.WriteLine("测试 关闭连接后 连接当前状态" + conn.State.ToString());
            }
            return 0;
        }

        //事务管理
        public static int test_Transaction()
        {
            XGConnection conn = new XGConnection();
            conn.ConnectionString = conn_xg;
            try
            {
                conn.Open();
                XGCommand cmd = new XGCommand();
                cmd.Connection = conn;
                cmd.CommandText = "insert into tp(pid,pname) values(11,'xugu');";
                XGTransaction t_Transaction = new XGTransaction(conn);
                cmd.ExecuteNonQuery();
                t_Transaction.Rollback();       //回滚操作

                cmd.CommandText = "SELECT COUNT(*) FROM tp WHERE pid=11;";
                if (Convert.ToInt16(cmd.ExecuteScalar()) != 0)
                {
                    throw new Exception("[ test_Transaction ]");
                }
            }
            catch (Exception Err)
            {
                Console.WriteLine("Error :");
                Console.WriteLine("\t{0}", Err.ToString());
                conn.Close();
                return -1;
            }
            return 0;
        }
    }
}
