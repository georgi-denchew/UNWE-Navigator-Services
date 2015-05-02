using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UNWE_Navigator_Services.Models;
using UNWE_Navigator_Services.Services;

namespace UNWE_Navigator_Services.Controllers
{
    public class RouteController : ApiController
    {
        public static ILog log = LogManager.GetLogger(typeof(RouteController));

        public string result = "";

        [HttpGet]
        public List<RouteModel> Get(string from, string to)
        {
            //log.Fatal("begin");
            result = "from: " + from + " to: " + to;
            List<RouteModel> resultA = null;
            try
            {
                resultA = LoadInfo(to, from);
                //   return resultA;
            }
            catch (Exception e)
            {
                result = e.Message;
            }

            log.Fatal("end");

            return resultA;
        }

        private List<RouteModel> LoadInfo(string to, string from)
        {

            this.result += " begin LoadInfo ";
            //log.Fatal("begin LoadInfo");
            List<RouteModel> result = new List<RouteModel>();

            string searchID = LibFunct.GetRandom();

            string routeId = string.Empty;
            string secpic = string.Empty;
            string rotation = string.Empty;
            string arrcoordsS = string.Empty;
            string floorsString = string.Empty;
            string roomsString = string.Empty;
            string picString = string.Empty;

            string room_fr = "";
            string room_to = "";

            //if (Request.Cookies["teach_from"] != null)
            {
                if (from == "")
                {
                    room_fr = "Информационни екрани";
                }
                else
                {
                    room_fr = from;
                }
            }
            //if (Request.Cookies["teach_to"] != null)
            {
                if (to == "")
                {
                    room_to = "Информационни екрани";
                }
                else
                {
                    room_to = to;
                }
            }

            if (!room_fr.Equals("") && !room_to.Equals("") && !room_fr.Contains("Err-") && !room_to.Contains("Err-"))
            {

                //get Section floor id for the start room
                string sql_sel = "Select IDSecFl, SecAlph,Floor,RoomID,IdSec from RoomsInSecFloors where RoomNum='" + room_fr + "'";
                DataTable r_from = DB_Functions.GetData(sql_sel);

                //get section floor id for the final destination
                string sql_sel2 = "Select IDSecFl, SecAlph,Floor,RoomID, IdSec from RoomsInSecFloors where RoomNum='" + room_to + "'";
                DataTable r_to = DB_Functions.GetData(sql_sel2);


                /// TODO: error messages
                if (r_from.Rows.Count < 1)
                {
                    //Err_msg.Text = "Няма зала/кабинет с такъв номер: " + room_fr;
                }
                else if (r_to.Rows.Count < 1)
                {
                    // Err_msg.Text = "Няма зала/кабинет с такъв номер: " + room_to;
                }
                else
                {
                    //create an array for holding temp information for floors
                    //4 dimensions - SecFlID, EnterPoint, LeavePoint, OrderNumber
                    String[,] floors = new String[1, 5];
                    //add first floor id
                    floors[0, 0] = r_from.Rows[0][0].ToString();
                    floors[0, 3] = "1";

                    //if secfl_from=secfl_to
                    if (r_from.Rows[0][0].ToString() == r_to.Rows[0][0].ToString())
                    {
                        floors[0, 1] = "";
                        floors[0, 2] = "";
                    }
                    else
                    {
                        string sqlGetRoute = "SELECT MIN(IDRouteOrder) FROM SectionRoute AS a" +
                            " WHERE ('" + r_from.Rows[0][4].ToString() + "' IN (SELECT IDSec FROM SectionRoute AS b WHERE (IDRouteOrder = a.IDRouteOrder))) AND " +
                            "('" + r_to.Rows[0][4].ToString() + "' IN (SELECT IDSec FROM SectionRoute AS c WHERE (IDRouteOrder = a.IDRouteOrder)))";
                        DataTable dt_route = DB_Functions.GetData(sqlGetRoute);
                        if (dt_route.Rows.Count > 0)
                        {
                            routeId = dt_route.Rows[0][0].ToString();
                            //Session["routeid"] = dt_route.Rows[0][0].ToString();
                            string check_pos = "Select Position from SectionRoute where IdSec=" + r_from.Rows[0][4].ToString() + " and IDRouteOrder=" + dt_route.Rows[0][0].ToString();
                            DataTable dt_posFrom = DB_Functions.GetData(check_pos);
                            string check_posTo = "Select Position from SectionRoute where IdSec=" + r_to.Rows[0][4].ToString() + " and IDRouteOrder=" + dt_route.Rows[0][0].ToString();
                            DataTable dt_posTo = DB_Functions.GetData(check_posTo);
                            if (dt_posFrom.Rows.Count > 0 && dt_posTo.Rows.Count > 0)
                            {

                                floors = GetFloors(dt_route.Rows[0][0].ToString(), floors, r_from.Rows[0][0].ToString(), r_to.Rows[0][0].ToString(), r_from.Rows[0][2].ToString(), r_to.Rows[0][2].ToString(), dt_posFrom.Rows[0][0].ToString(), dt_posTo.Rows[0][0].ToString());
                            }
                        }
                    }

                    // roomsString = r_from.Rows[0][3].ToString() + ">" + r_to.Rows[0][3].ToString();
                    string[] rooms = new string[] { r_from.Rows[0][3].ToString(), r_to.Rows[0][3].ToString() };

                    //Session["floors"] = "";
                    for (int i = 0; i < floors.GetLength(0); i++)
                    {
                        floorsString += ">" + floors[i, 0];
                        //Session["floors"] += ">" + floors[i, 0];

                        string sql = "Insert into TempPathData (SearchID,IDSecFl,EnterPoint,LeavePoint,OrderNum) Values ('" + searchID + "'," + floors[i, 0] + ",'" + floors[i, 1] + "','" + floors[i, 2] + "'," + (i + 1) + ")";
                        DB_Functions.GetData(sql);

                        RouteModel routeModel = GetRoute(floors[i, 0], rooms[0], rooms[1], searchID);

                        result.Add(routeModel);
                    }


                    floorsString = floorsString.Substring(1);

                    //Session["floors"] = Session["floors"].ToString().Substring(1);
                    //HttpCookie cookie_tid = new HttpCookie("floors", Session["floors"].ToString());
                    //Response.SetCookie(cookie_tid);
                    //Session["rooms"] = r_from.Rows[0][3].ToString() + ">" + r_from2.Rows[0][3].ToString();
                    //HttpCookie cookie_r = new HttpCookie("rooms", Session["rooms"].ToString());
                    //Response.SetCookie(cookie_r);
                    //lbl_SearchRes.Text = DB_Functions.GetDestinationInfo(Convert.ToInt32(r_from2.Rows[0][3].ToString()));



                    //string sel = "Select Pic,SecPic,Rotation,MarkerPath from SecFlPics where IDSecFl=" + r_to.Rows[0][0].ToString();//Request.QueryString["id"].ToString();
                    //DataTable dt = DB_Functions.GetData(sel);
                    //if (dt.Rows.Count > 0)
                    //{
                    //    if (dt.Rows[0][2].ToString().Equals("1") || dt.Rows[0][2].ToString().Equals("2"))
                    //    {
                    //        picString = dt.Rows[0][0].ToString().Replace("/", "/r");
                    //        //Session["pic"] = dt.Rows[0][0].ToString().Replace("/", "/r");
                    //    }
                    //    else
                    //    {
                    //        picString = dt.Rows[0][0].ToString();
                    //        //Session["pic"] = dt.Rows[0][0].ToString();
                    //    }

                    //    secpic = dt.Rows[0][1].ToString();
                    //    rotation = dt.Rows[0][2].ToString();
                    //    //Session["secpic"] = dt.Rows[0][1].ToString();
                    //    //Session["rotation"] = dt.Rows[0][2].ToString();

                    //    //get all floors arrow coordinates for big picture
                    //    string fl_arrcoords = "";
                    //    for (int i = 0; i < floors.GetLength(0); i++)
                    //    {
                    //        fl_arrcoords += "," + floors[i, 0];

                    //    } fl_arrcoords = fl_arrcoords.Substring(1);
                    //    //string sql = "Select MarkerPath from SecFlPics INNER JOIN TempPathData ON SecFlPics.IDSecFl = TempPathData.IDSecFl" +
                    //    //    " where SecFlPics.IDSecFl in (" + fl_arrcoords + ") and searchID='" + Request.Cookies["SearchID"].Value.ToString() + "' ORDER BY TempPathData.OrderNum";

                    //    string sql = "Select MarkerPath from SecFlPics INNER JOIN TempPathData ON SecFlPics.IDSecFl = TempPathData.IDSecFl" +
                    //        " where SecFlPics.IDSecFl in (" + fl_arrcoords + ") and searchID='" + searchID + "' ORDER BY TempPathData.OrderNum";


                    //    DataTable arr_coords = DB_Functions.GetData(sql);
                    //    for (int i = 0; i < arr_coords.Rows.Count; i++)
                    //    {
                    //        arrcoordsS += ">" + arr_coords.Rows[i][0].ToString();
                    //        //Session["arrcoordsS"] += ">" + arr_coords.Rows[i][0].ToString();
                    //    }
                    //    arrcoordsS = arrcoordsS.Substring(1);

                    //    //Session["arrcoordsS"] = Session["arrcoordsS"].ToString().Substring(1);


                    //    //Session["rooms"].ToString().Split('>');
                    //    List<string> links = LoadLinks(floorsString);



                    //    //btn_ShowPath.Visible = true;
                    //    // Err_msg.Text = "";
                    //}
                    //else
                    //{
                    //    // Err_msg.Text = "Липсва изображение за етажа";
                    //    // btn_ShowPath.Visible = false;
                    //}
                }
            }
            else
            {
                //if (room_fr.Equals("") || room_fr.Contains("Err-"))
                //    Err_msg.Text = "За този преподавател не е указан кабинет: " + teach_from.Text;
                //else if (room_to.Equals("") || room_to.Contains("Err-"))
                //    Err_msg.Text = "За този преподавател не е указан кабинет: " + teach_to.Text;
                //btn_ShowPath.Visible = false;
            }

            this.result += "end LoadInfo ";
            log.Debug("end LoadInfo");


            return result;

            //return new RouteModel
            //{
            //    ArrCoords = arrcoordsS,
            //    FloorsString = floorsString,
            //    PicString = picString,
            //    RoomsString = roomsString,
            //    Rotation = rotation,
            //    SecPic = secpic
            //};
        }

        private RouteModel GetRoute(string floor, string fromRoom, string toRoom, string searchID)
        {
            this.result += "begin GetRoute ";

            log.Debug("begin GetRoute");

            RouteModel result = new RouteModel();

            string getFloors = "Select SecAlph,Floor from SectionFloorNames where IDSecFl=" + floor;
            DataTable dt1 = DB_Functions.GetData(getFloors);
            
            if (dt1.Rows.Count > 0)
            {
                result.FloorSection = dt1.Rows[0][0].ToString();
                result.Floor = Convert.ToInt32(dt1.Rows[0][1]);
            }


            string sel = "Select Pic,SecPic,Rotation,MarkerPath from SecFlPics where IDSecFl=" + floor;
            result.FloorSectionID = floor;
            result.RoomFromID = int.Parse(fromRoom);
            result.RoomToID = int.Parse(toRoom);

            DataTable dt = DB_Functions.GetData(sel);

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0][2].ToString().Equals("1") || dt.Rows[0][2].ToString().Equals("2"))
                {
                    result.PictureFileName = dt.Rows[0][0].ToString().Replace("/", "/r");
                    //Session["pic"] = dt.Rows[0][0].ToString().Replace("/", "/r");
                }
                else
                {
                    result.PictureFileName = dt.Rows[0][0].ToString();
                    //Session["pic"] = dt.Rows[0][0].ToString();
                }

                //result.SecPic = dt.Rows[0][1].ToString();
                //Session["secpic"] = dt.Rows[0][1].ToString();
                result.Rotation = dt.Rows[0][2].ToString();
                //Session["rotation"] = dt.Rows[0][2].ToString();
                result.ArrCoords = dt.Rows[0][3].ToString();
                //Session["arrcoords"] = dt.Rows[0][3].ToString();


                List<Point> pathCoords = Calculate(fromRoom, toRoom, floor, searchID);
                result.PathPoints = pathCoords;
            }

            this.result += "end GetRoute ";
            log.Debug("end GetRoute");

            return result;
        }

        private List<Point> Calculate(string r1, string r2, string sec_fl, string searchID)
        {
            this.result += "begin Calculate ";
            log.Debug("begin Calculate");

            int step = 25;

            List<Point> result = new List<Point>();

            DataTable dt_mp = DB_Functions.GetData("Select PathCoords from AllPaths where IDSecFl=" + sec_fl);
            String[] mainPath = dt_mp.Rows[0][0].ToString().Split('>');
            if (mainPath.Length > 0)
            {

                string[,] fl_rooms = new string[0, 3];
                //add room path if the first room is on this floor

                DataTable dt_rooms = DB_Functions.GetData("Select RoomID,RoomPath from SecFlRooms where IDSecFl=" + sec_fl + " and RoomID=" + r1);
                if (dt_rooms.Rows.Count > 0)
                {
                    LibFunct.ResizeArray3(ref fl_rooms, fl_rooms.GetLength(0) + 1);
                    fl_rooms[0, 0] = dt_rooms.Rows[0][0].ToString();
                    fl_rooms[0, 1] = dt_rooms.Rows[0][1].ToString();
                    fl_rooms[0, 2] = "1";
                }
                //add point path if any of the points on the floor are used
                string sql = "Select PointName,PointPath,OrderNum,'1' AS NumOrd from TempPathData INNER JOIN EntryPoints ON TempPathData.IDSecFl = EntryPoints.IdSecFl " +
                    "where TempPathData.IDSecFl=" + sec_fl + " and SearchID='" +
                    searchID
                    //Request.Cookies["SearchID"].Value.ToString() 
                    + "' and IDEntryPoint=EnterPoint " +
                    " union Select PointName,PointPath,OrderNum,'2' AS NumOrd from TempPathData INNER JOIN EntryPoints ON TempPathData.IDSecFl = EntryPoints.IdSecFl " +
                    "where TempPathData.IDSecFl=" + sec_fl + " and SearchID='" +
                    searchID
                    //Request.Cookies["SearchID"].Value.ToString() 
                    + "' and IDEntryPoint=LeavePoint order by NumOrd";// where EnterPoint is not '' and LeavePoint is not ''

                DataTable dt_rp = DB_Functions.GetData(sql);
                int fin_i = dt_rp.Rows.Count + fl_rooms.GetLength(0);
                int i_stval = fl_rooms.GetLength(0);
                int dt_rp_cnt = 0;
                for (int i = i_stval; i < fin_i; i++)
                {
                    LibFunct.ResizeArray3(ref fl_rooms, fl_rooms.GetLength(0) + 1);
                    fl_rooms[i, 0] = dt_rp.Rows[dt_rp_cnt][0].ToString();
                    fl_rooms[i, 1] = dt_rp.Rows[dt_rp_cnt][1].ToString();
                    fl_rooms[i, 2] = dt_rp.Rows[dt_rp_cnt][2].ToString();
                    dt_rp_cnt++;
                }

                //add room path if the second room is on this floor
                string sql2 = "Select RoomID,RoomPath from SecFlRooms where IDSecFl=" + sec_fl + " and RoomID=" + r2;

                DataTable dt_rooms2 = DB_Functions.GetData(sql2);
                if (dt_rooms2.Rows.Count > 0)
                {
                    LibFunct.ResizeArray3(ref fl_rooms, fl_rooms.GetLength(0) + 1);
                    fl_rooms[1, 0] = dt_rooms2.Rows[0][0].ToString();
                    fl_rooms[1, 1] = dt_rooms2.Rows[0][1].ToString();
                    fl_rooms[1, 2] = "99";
                }

                if (fl_rooms.GetLength(0) < 2)
                {
                    // Err_msg.Text = "Възникна грешка (проблем с данните за етаж:" + sec_fl + ")";
                }
                else
                {
                    int ix_start = 0;
                    int ix_end = mainPath.Length - 1;
                    string[] pathCoords = new string[0];


                    //   try
                    // {
                    //find closest path point for start
                    for (int i = 0; i < mainPath.Length; i++)
                    {
                        int x1_diff = Convert.ToInt32(mainPath[i].Split(new char[] { ';' })[0]) - Convert.ToInt32(fl_rooms[0, 1].Split('>')[0].Split(new char[] { ';' })[0]);
                        int y1_diff = Convert.ToInt32(mainPath[i].Split(new char[] { ';' })[1]) - Convert.ToInt32(fl_rooms[0, 1].Split('>')[0].Split(new char[] { ';' })[1]);
                        if (x1_diff <= step && x1_diff >= step * -1 && (y1_diff >= (step * -1) && y1_diff <= step))//y1_diff == 0)
                        {
                            ix_start = i;
                            break;
                        }
                        else if ((x1_diff >= step * -1 && x1_diff <= step) && y1_diff <= step && y1_diff >= step * -1)//x1_diff == 0
                        {
                            ix_start = i;
                            break;
                        }
                    }
                    //}
                    //catch (Exception exx)
                    //{
                    //    //Err_msg.Text = "Възникна грешка с началната точка, моля опитайте отново";
                    //}

                    //try
                    //{
                    //find closest path point for start
                    for (int i = 0; i < mainPath.Length; i++)
                    {
                        int x1_diff = Convert.ToInt32(mainPath[i].Split(new char[] { ';' })[0]) - Convert.ToInt32(fl_rooms[1, 1].Split('>')[0].Split(new char[] { ';' })[0]);
                        int y1_diff = Convert.ToInt32(mainPath[i].Split(new char[] { ';' })[1]) - Convert.ToInt32(fl_rooms[1, 1].Split('>')[0].Split(new char[] { ';' })[1]);
                        if (x1_diff <= step && x1_diff >= step * -1 && (y1_diff >= step * -1 && y1_diff <= step))//y1_diff == 0)
                        {
                            ix_end = i;
                            break;
                        }
                        else if ((x1_diff >= step * -1 && x1_diff <= step) && y1_diff <= step && y1_diff >= step * -1)//x1_diff == 0
                        {
                            ix_end = i;
                            break;
                        }
                    }
                    //}
                    //catch (Exception exx)
                    //{
                    // //   Err_msg.Text = "Възникна грешка с крайната точка, моля опитайте отново";
                    //}

                    //get start point/room coords -> add it as 1st array item

                    string[] r1_coords = fl_rooms[0, 1].Split('>');
                    int curr_size = pathCoords.Length;
                    Array.Resize(ref pathCoords, pathCoords.Length + r1_coords.Length);
                    for (int i = 0; i < r1_coords.Length; i++)
                    {
                        pathCoords[i + curr_size] = r1_coords[(r1_coords.Length - 1) - i];
                    }



                    //add to the array all the points from main path that suit the need

                    if (ix_start > ix_end)
                    {
                        int ss = ix_start;
                        for (int j = ss; j > ix_end; j = j - 1)
                        {
                            Array.Resize(ref pathCoords, pathCoords.Length + 1);
                            pathCoords[pathCoords.Length - 1] = mainPath[j];
                        }
                    }
                    else
                    {
                        int ss = ix_start;
                        for (int j = ss; j < ix_end; j++)
                        {
                            Array.Resize(ref pathCoords, pathCoords.Length + 1);
                            pathCoords[pathCoords.Length - 1] = mainPath[j];
                        }
                    }

                    //check if we have room as an end point -> add it as last array item

                    string[] r2_coords = fl_rooms[1, 1].Split('>');
                    curr_size = pathCoords.Length;
                    Array.Resize(ref pathCoords, pathCoords.Length + r2_coords.Length);
                    for (int i = 0; i < r2_coords.Length; i++)
                    {
                        pathCoords[i + curr_size] = r2_coords[i];
                    }

                    //remove repeated values

                    string[] refinedPath = new string[1];
                    refinedPath[0] = pathCoords[0];

                    for (int i = 0; i < pathCoords.Length; i++)
                    {
                        int x1R = int.Parse(pathCoords[i].Substring(0, pathCoords[i].IndexOf(";")));
                        int y1R = int.Parse(pathCoords[i].Substring(pathCoords[i].IndexOf(";") + 1));

                        Point point = new Point
                        {
                            X = x1R,
                            Y = y1R
                        };

                        result.Add(point);

                        Array.Resize(ref refinedPath, refinedPath.Length + 1);
                        refinedPath[refinedPath.Length - 1] = pathCoords[i];


                        for (int j = 1; j < 10; j++)
                        {
                            if (i + 1 + j < pathCoords.Length)
                            {
                                int x1R3 = int.Parse(pathCoords[i + 1 + j].Substring(0, pathCoords[i + j + 1].IndexOf(";")));
                                int y1R3 = int.Parse(pathCoords[i + 1 + j].Substring(pathCoords[i + j + 1].IndexOf(";") + 1));

                                if (Math.Abs(x1R - x1R3) + Math.Abs(y1R - y1R3) < 35)
                                {
                                    i = i + j;
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    this.result += "end Calculate ";
                    log.Debug("end Calculate");

                    return result;
                }
            }

            this.result += "end Calculate null ";
            log.Debug("end Calculate null");

            return null;

        }

        private List<string> LoadLinks(string floorString)
        {
            List<string> result = new List<string>();
            //this.result += "begin LoadLinks ";
            log.Debug("begin LoadLinks");

            string link = string.Empty;

            if (floorString != null)
            {
                string getFloors = "Select SecAlph,Floor from SectionFloorNames where IDSecFl=" + floorString;
                DataTable dt = DB_Functions.GetData(getFloors);
                if (dt.Rows.Count > 0)
                {
                    link = "корпус " + dt.Rows[0][0] + " ет." + dt.Rows[0][1] + "; ";
                    link = "корпус " + dt.Rows[0][0] + " ет." + dt.Rows[0][1] + "; ";

                    result.Add(link);
                }
            }
            this.result += "end LoadLinks ";
            log.Debug("end LoadLinks");

            return result;
        }


        private string[,] GetFloors(string routeid, String[,] floors_info, string secfl_from, string secfl_to, string floor_from, string floor_to, string pos_from, string pos_to)
        {
            this.result += "begin GetFloors ";
            log.Debug("begin GetFloors");

            //get all connectors from current floor           
            string sql_getidfl = "Select PointName,IDEntryPoint from EntryPointsView where IDSecFl=" + secfl_from + " and IDRouteOrder=" + routeid + " order by Position";
            if (int.Parse(pos_from) < int.Parse(pos_to))
            {
                sql_getidfl += " asc";
            }
            else
            {
                sql_getidfl += " desc";
            }
            DataTable temp_ids = DB_Functions.GetData(sql_getidfl);
            //for each of the connectors from current floor
            for (int i = 0; i < temp_ids.Rows.Count; i++)
            {
                bool check_formiss = false;

                //get the other floor for this connector
                string connector_name = temp_ids.Rows[i][0].ToString();
                string sql_getidflnext = "Select IDSecFl, IDEntryPoint,Floor, Position, IdSec from EntryPointsView where PointName='" + temp_ids.Rows[i][0].ToString() + "' and IDSecFl<>" + secfl_from + " and IDRouteOrder=" + routeid;
                DataTable temp_nextfl = DB_Functions.GetData(sql_getidflnext);



                //check for valid new floor
                //possibilities:
                //1-no new secfl -> dead end, miss the connector from the current loop
                //2-secfl_from already used -> miss the connector from the current loop
                //3-secfl_from going in wrong direction -> miss the connector from the current loop
                //4-secfl_from in right direction -> go on with GetFloors


                if (temp_nextfl.Rows.Count == 0)// && temp_nextfl.Rows[0][0].ToString() != secfl_to)
                {
                    check_formiss = true;
                }
                else
                {
                    string secid = temp_nextfl.Rows[0][4].ToString();
                    if (temp_nextfl.Rows[0][4].ToString() == "6" && (
                        Math.Abs(Convert.ToInt32(temp_nextfl.Rows[0][3].ToString()) - Convert.ToInt32(pos_to)) != 0 ||
                        Math.Abs((Convert.ToInt32(temp_nextfl.Rows[0][2].ToString()) - Convert.ToInt32(floor_to))) != 0))
                    {
                        check_formiss = true;
                    }
                    else
                    {
                        //case #3
                        //height
                        string floor_curr_checking = temp_nextfl.Rows[0][2].ToString();
                        if (Math.Abs((Convert.ToInt32(floor_from) - Convert.ToInt32(floor_to))) < Math.Abs((Convert.ToInt32(temp_nextfl.Rows[0][2].ToString()) - Convert.ToInt32(floor_to))))
                        {
                            check_formiss = true;
                        }
                        //width
                        string pos_curr_checking = temp_nextfl.Rows[0][3].ToString();
                        if (Math.Abs((Convert.ToInt32(pos_from) - Convert.ToInt32(pos_to))) < Math.Abs((Convert.ToInt32(temp_nextfl.Rows[0][3].ToString()) - Convert.ToInt32(pos_to))))
                        {
                            check_formiss = true;
                        }

                        //case #1
                        if (temp_nextfl.Rows.Count > 0)
                        {
                            //get the connectors of the potential next floor
                            string sql_getidflnext2 = "Select IDSecFl, IDEntryPoint,Floor, Position, IdSec from EntryPointsView where PointName <> '" + temp_ids.Rows[i][0].ToString() + "' and IDSecFl=" + temp_nextfl.Rows[0][0].ToString() + " and IDRouteOrder=" + routeid;
                            DataTable temp_nextfl2 = DB_Functions.GetData(sql_getidflnext2);

                            if (temp_nextfl.Rows[0][0].ToString() != secfl_to && temp_nextfl2.Rows.Count == 0)
                            {
                                check_formiss = true;
                            }
                        }


                    }
                    //case #2     
                    for (int j = 0; j < floors_info.GetLength(0); j++)
                    {
                        if (floors_info[j, 0] == temp_nextfl.Rows[0][0].ToString())
                        {
                            check_formiss = true;
                            break;
                        }
                    }

                }

                //if  miss the current loop connector -  continue with the next loop
                if (check_formiss)
                {
                    //final_array = floors_info;
                    continue;
                }
                string checking_for_secflid = temp_nextfl.Rows[0][0].ToString();
                //case #1 or #4
                //complete current floor info (leaving point)                    
                floors_info[floors_info.GetLength(0) - 1, 2] = temp_ids.Rows[i][1].ToString();



                // case #4 (add new floor info (floor id, entering point))
                LibFunct.ResizeArray(ref floors_info, floors_info.GetLength(0) + 1);
                floors_info[floors_info.GetLength(0) - 1, 0] = temp_nextfl.Rows[0][0].ToString();
                floors_info[floors_info.GetLength(0) - 1, 1] = temp_nextfl.Rows[0][1].ToString();
                floors_info[floors_info.GetLength(0) - 1, 3] = floors_info.GetLength(0).ToString();
                floors_info[floors_info.GetLength(0) - 1, 4] = "0";

                //case #0 (if this is the final floor - exit)
                if (temp_nextfl.Rows[0][0].ToString() == secfl_to)
                {
                    floors_info[floors_info.GetLength(0) - 1, 4] = "1";
                    return floors_info;
                }

                String[,] newarr = GetFloors(routeid, floors_info, temp_nextfl.Rows[0][0].ToString(), secfl_to, temp_nextfl.Rows[0][2].ToString(), floor_to, temp_nextfl.Rows[0][3].ToString(), pos_to);
                int p = newarr.GetLength(0);
                if (newarr.GetLength(0) > 0 && newarr[newarr.GetLength(0) - 1, 4].ToString() == "1")
                {
                    floors_info = newarr;
                    return floors_info;
                }

            }

            this.result += "end GetFloors ";
            log.Debug("end GetFloors");


            return floors_info;
        }
    }
}
