using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace UNWE_Navigator_Services.Services
{
    public class DB_Functions
    {
        public static DataTable GetData(string sql)
        {
            DataTable dt = new DataTable();

            string connStr = ConfigurationManager.ConnectionStrings["UNWEbuildConnectionString"].ConnectionString;
            SqlConnection con = new SqlConnection(connStr);
            SqlCommand sqlP = new SqlCommand(sql, con);

            try
            {
                con.Open();
                SqlDataReader dr = sqlP.ExecuteReader();

                if (dr.HasRows)
                {
                    dt.Load(dr);
                }
                con.Close();

            }
            catch (SqlException ex)
            {
                con.Close();
            }
            return dt;
        }

        public static string InsertPathInfo(String path, int IDSecFl, string rpName, int type)
        {
            string connStr = ConfigurationManager.ConnectionStrings["UNWEbuildConnectionString"].ConnectionString;
            SqlConnection con = new SqlConnection(connStr);
            string sqlCommandText = "";

            switch (type)
            {
                case 1: sqlCommandText = "Update AllPaths Set PathCoords='" + path + "' where IDSecFl=" + IDSecFl;
                    break;
                case 2: sqlCommandText = "Update SecFlRooms SET RoomPath='" + path + "' where RoomNum=N'" + rpName + "' and IdSecFl=" + IDSecFl + "";
                    break;
                case 3: sqlCommandText = "Update EntryPoints SET PointPath='" + path + "' where IdSecFl=" + IDSecFl + " and PointName='" + rpName + "'";
                    break;
                default: return "Грешен тип";
            }

            SqlCommand sqlP = new SqlCommand(sqlCommandText, con);
            try
            {
                con.Open();
                sqlP.ExecuteNonQuery();
                return "Информацията е добавена успешно";
            }
            catch (SqlException exx)
            {
                con.Close();
                return exx.Message;
            }
        }

        public static List<String> CheckData()
        {
            List<String> errorMessage = new List<String>();

            string sql_selsect = "SELECT IdSecFl,PathCoords FRom AllPaths";
            DataTable dt_secfl = GetData(sql_selsect);
            for (int i = 0; i < dt_secfl.Rows.Count; i++)
            {
                //put the main path of the sectionfloor into an array
                String[] mainPath = dt_secfl.Rows[i][1].ToString().Split('>');

                //Check all rooms from the sectionfloor
                string sql_selPoints = "select  PointName,PointPath FROM EntryPoints WHERE (IdSecFl = " + dt_secfl.Rows[i][0].ToString() + ")";
                DataTable dt_points = GetData(sql_selPoints);
                for (int j = 0; j < dt_points.Rows.Count; j++)
                {
                    string pointname = dt_points.Rows[j][0].ToString();
                    string pointPath = dt_points.Rows[j][1].ToString();
                    //1
                    int ix_start = -1;
                    int step = 25;
                    string[] pathCoords = new string[0];
                    try
                    {
                        //find closest path point for start
                        for (int f = 0; f < mainPath.Length; f++)
                        {
                            string pointss = dt_points.Rows[j][1].ToString();
                            int x1_diff = Convert.ToInt32(mainPath[f].Split(new char[] { ';' })[0]) - Convert.ToInt32(dt_points.Rows[j][1].ToString().Split('>')[0].Split(new char[] { ';' })[0]);
                            int y1_diff = Convert.ToInt32(mainPath[f].Split(new char[] { ';' })[1]) - Convert.ToInt32(dt_points.Rows[j][1].ToString().Split('>')[0].Split(new char[] { ';' })[1]);
                            if (x1_diff <= step && x1_diff >= step * -1 && (y1_diff >= (step * -1) && y1_diff <= step))//y1_diff == 0)
                            {
                                ix_start = f;
                                break;
                            }
                            else if ((x1_diff >= step * -1 && x1_diff <= step) && y1_diff <= step && y1_diff >= step * -1)//x1_diff == 0
                            {
                                ix_start = f;
                                break;
                            }
                        }
                        if (ix_start == -1)
                        {
                            errorMessage.Add("IDSec=" + dt_secfl.Rows[i][0].ToString() + " - Не е намерено съвпадение за свързваща точка " + dt_points.Rows[j][0].ToString());
                        }
                    }
                    catch (Exception exx)
                    {
                        if (pointPath == "")
                        {
                            errorMessage.Add("IDSec=" + dt_secfl.Rows[i][0].ToString() + " Стая " + pointname + " няма координати");
                        }
                        else
                        {
                            errorMessage.Add("IDSec=" + dt_secfl.Rows[i][0].ToString() + "Point-" + pointname + "Path-" + pointPath + " - Възникна грешка:" + exx.Message);
                        }
                    }
                }
                //end of checking rooms

                //Check all EntryPoints
                string sql_selRooms = "select  RoomNum,RoomPath FROM SecFlRooms WHERE (IdSecFl = " + dt_secfl.Rows[i][0].ToString() + ")";
                DataTable dt_rooms = GetData(sql_selRooms);
                for (int j = 0; j < dt_rooms.Rows.Count; j++)
                {
                    string pointname = dt_rooms.Rows[j][0].ToString();
                    string pointPath = dt_rooms.Rows[j][1].ToString();
                    //2
                    int ix_start = -1;
                    int step = 25;
                    string[] pathCoords = new string[0];
                    try
                    {
                        //find closest path point for start
                        for (int f = 0; f < mainPath.Length; f++)
                        {
                            int x1_diff = Convert.ToInt32(mainPath[f].Split(new char[] { ';' })[0]) - Convert.ToInt32(dt_rooms.Rows[j][1].ToString().Split('>')[0].Split(new char[] { ';' })[0]);
                            int y1_diff = Convert.ToInt32(mainPath[f].Split(new char[] { ';' })[1]) - Convert.ToInt32(dt_rooms.Rows[j][1].ToString().Split('>')[0].Split(new char[] { ';' })[1]);
                            if (x1_diff <= step && x1_diff >= step * -1 && (y1_diff >= (step * -1) && y1_diff <= step))//y1_diff == 0)
                            {
                                ix_start = f;
                                break;
                            }
                            else if ((x1_diff >= step * -1 && x1_diff <= step) && y1_diff <= step && y1_diff >= step * -1)//x1_diff == 0
                            {
                                ix_start = f;
                                break;
                            }
                        }
                        if (ix_start == -1)
                        {
                            errorMessage.Add("IDSec=" + dt_secfl.Rows[i][0].ToString() + " -Не е намерено съвпадение за стая " + dt_rooms.Rows[j][0].ToString());
                        }
                    }
                    catch (Exception exx)
                    {
                        if (pointPath == "")
                        {
                            errorMessage.Add("IDSec=" + dt_secfl.Rows[i][0].ToString() + " Точка " + pointname + " няма координати");
                        }

                        else
                        {
                            errorMessage.Add("IDSec=" + dt_secfl.Rows[i][0].ToString() + "Point-" + pointname + "Path-" + pointPath + " - Възникна грешка:" + exx.Message);
                        }
                    }
                }
                // check paths data
                //paths for floors
                string sql_selsectFl = "select  IDSecFl,PathCoords FROM AllPaths WHERE (IdSecFl = " + dt_secfl.Rows[i][0].ToString() + ")";
                DataTable dt_floors = GetData(sql_selsectFl);
                for (int j = 0; j < dt_floors.Rows.Count; j++)
                {
                    try
                    {
                        string[] coords = dt_floors.Rows[j][1].ToString().Split(new char[] { ';', '>' });
                        foreach (string point in coords)
                        {
                            int checkvalue = int.Parse(point);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage.Add("IDSec=" + dt_secfl.Rows[i][0].ToString() + " Етаж -" + dt_floors.Rows[j][0].ToString() + " съдържа невалидни координати");
                    }
                }
                //paths for rooms
                sql_selRooms = "select  RoomNum,RoomPath FROM SecFlRooms WHERE (IdSecFl = " + dt_secfl.Rows[i][0].ToString() + ")";
                dt_rooms = GetData(sql_selRooms);
                for (int j = 0; j < dt_rooms.Rows.Count; j++)
                {
                    try
                    {
                        string[] coords = dt_rooms.Rows[j][1].ToString().Split(new char[] { ';', '>' });
                        foreach (string point in coords)
                        {
                            int checkvalue = int.Parse(point);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage.Add("IDSec=" + dt_secfl.Rows[i][0].ToString() + " Зала -" + dt_rooms.Rows[j][0].ToString() + " съдържа невалидни координати");
                    }
                }

                //paths for entrypoints
                sql_selPoints = "select  PointName,PointPath FROM EntryPoints WHERE (IdSecFl = " + dt_secfl.Rows[i][0].ToString() + ")";
                dt_points = GetData(sql_selPoints);
                for (int j = 0; j < dt_points.Rows.Count; j++)
                {
                    try
                    {
                        string[] coords = dt_points.Rows[j][1].ToString().Split(new char[] { ';', '>' });
                        foreach (string point in coords)
                        {
                            int checkvalue = int.Parse(point);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage.Add("IDSec=" + dt_secfl.Rows[i][0].ToString() + " Входна точка -" + dt_points.Rows[j][0].ToString() + " съдържа невалидни координати");
                    }
                }

                //end of checking points
            }

            if (errorMessage.Count == 0)
            {
                errorMessage.Add("Няма намерени грешки в данните");
            }
            return errorMessage;
        }

        public static DataTable CheckTeachers()
        {
            DataTable teach_info = new DataTable();

            string sql_selTeach = "Select TeachID, TeachName,Room from View_ListTeachers where Room='NULL' or (Select Count(*) from SecFlRooms where RoomNum=Room)=0 order by room";
            teach_info = GetData(sql_selTeach);

            return teach_info;
        }

        public static String GetDestinationInfo(int roomid)
        {
            string connStr = ConfigurationManager.ConnectionStrings["UNWEbuildConnectionString"].ConnectionString;
            SqlConnection con = new SqlConnection(connStr);
            SqlCommand sqlP = new SqlCommand("STPr_GetInfo", con);
            sqlP.CommandType = CommandType.StoredProcedure;

            sqlP.Parameters.AddWithValue("final_room", roomid);

            try
            {
                con.Open();
                SqlDataReader dr = sqlP.ExecuteReader();

                if (dr.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Load(dr);


                    con.Close();
                    return dt.Rows[0][0].ToString();
                }
            }
            catch (SqlException)
            {
                con.Close();
            }
            return "Error with info getting";
        }
        public static String GetTeachId(string textBoxValue)
        {
            string connStr = ConfigurationManager.ConnectionStrings["UNWEbuildConnectionString"].ConnectionString;
            SqlConnection con = new SqlConnection(connStr);
            SqlCommand sqlP = new SqlCommand("STPr_GetTeachID", con);
            sqlP.CommandType = CommandType.StoredProcedure;

            sqlP.Parameters.AddWithValue("checkName", textBoxValue);

            try
            {
                con.Open();
                SqlDataReader dr = sqlP.ExecuteReader();

                if (dr.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Load(dr);


                    con.Close();
                    return dt.Rows[0][0].ToString();
                }
            }
            catch (SqlException e)
            {
                con.Close();
                return e.Message;
            }
            return "Грешка: Проблем с връзката към базата данни";
        }

        public static String GetDestination(string textBoxValue)
        {
            string connStr = ConfigurationManager.ConnectionStrings["UNWEbuildConnectionString"].ConnectionString;
            SqlConnection con = new SqlConnection(connStr);
            SqlCommand sqlP = new SqlCommand("STPr_GetRoom", con);
            sqlP.CommandType = CommandType.StoredProcedure;

            sqlP.Parameters.AddWithValue("checkName", textBoxValue);

            try
            {
                con.Open();
                SqlDataReader dr = sqlP.ExecuteReader();

                if (dr.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Load(dr);


                    con.Close();
                    return dt.Rows[0][0].ToString();
                }
            }
            catch (SqlException)
            {
                con.Close();
            }
            return "Грешка: Проблем с връзката към базата данни";
        }

        public static string GetPic(string secflid)
        {
            string sql = "Select Pic from SectionFloors where IDSecFl=" + secflid;
            DataTable dt = GetData(sql);
            return dt.Rows[0][0].ToString();
        }
    }
}