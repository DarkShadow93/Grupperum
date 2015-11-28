﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrupperumServer.ModelLayer;

namespace GrupperumServer.DBConFold
{
    public class DBCtrl
    {
        public DBCon dbCon = new DBCon();

        public DBCtrl()
        {

        }

        public Student GetStudentById(int id)
        {
            SqlDataReader rs = dbCon.ExecuteStringGet("SELECT * FROM Student where id=" + id + ");");



            int tempId = 0;
            string tempName = null;
            string tempPassword = null;
            if (rs.HasRows)
            {
                while (rs.Read())
                {
                    tempId = (int)rs.GetValue(0);
                    tempName = rs.GetString(1);
                    tempPassword = rs.GetString(2);
                }
            }

            Student tempStudent = new Student(tempId, tempName, tempPassword);

            return tempStudent;
        }

        public Class GetClassFromStudentId(int id)
        {
            SqlDataReader rs = dbCon.ExecuteStringGet("SELECT * FROM Class where id=(SELECT classId FROM Student where id=" + id + ");");

            int tempId = 0;
            string tempName = null;
            if(rs.HasRows)
            {
                while (rs.Read())
                {
                    tempId = (int)rs.GetValue(0);
                    tempName = rs.GetString(1);
                }
            }
            
            Class tempClass = new Class(tempId, tempName);
                
            return tempClass;
         }

        public List<Student> GetStudentsFromClassId(int id)
        {
            int tempId = 0;
            string tempName = null;
            List<Student> tempList = new List<Student>();

            SqlDataReader rs = dbCon.ExecuteStringGet("SELECT * FROM student WHERE ClassID = " + id);

            if(rs.HasRows)
            {
                while (rs.Read())
                {
                    tempId = (int)rs.GetValue(0);
                    tempName = (string)rs.GetValue(2);
                    Student tempStudent = new Student(tempId, tempName);
                    tempList.Add(tempStudent);
                }
            }

            return tempList;
        }

        internal bool UpdateGroupRoom(string name, bool whiteboard, bool monitor)
        {
            int whiteboardBit = 0;
            int monitorBit = 0;

            if(whiteboard)
            {
                whiteboardBit = 1;
            }
            if(monitor)
            {
                monitorBit = 1;
            }

            string sqlCommand = string.Format("UPDATE GroupRoom SET whiteboard={0}, monitor={1} WHERE name='{2}'", whiteboardBit, monitorBit, name);
            return dbCon.ExecuteStringPut(sqlCommand);
        }

        internal List<string> GetGroupRoomNames()
        {
            List<string> roomNames = new List<string>();

            string sqlCommand = "SELECT [name] FROM [GroupRoom]";
            SqlDataReader rs = dbCon.ExecuteStringGet(sqlCommand);

            while(rs.Read())
            {
                roomNames.Add(rs.GetString(0));
            }

            return roomNames;
        }

        public bool CreateGroup(string name, List<int> studentId)
        {
            int groupId = 0;
            int groupSize = studentId.Count();
            bool done = false;
            SqlDataReader rs = dbCon.ExecuteStringGet("INSERT INTO [Group] (name, size) OUTPUT Inserted.id VALUES('" + name + "', '" + groupSize + "');");

            while (rs.Read())
            {
                groupId = (int)rs.GetValue(0);
                done = true;
            }

            foreach (int id in studentId)
            {
                done = dbCon.ExecuteStringPut("UPDATE Student SET groupId = " + groupId + " WHERE id = " + id + ";");
            }

            return done;
        }

        public bool CreateGroupRoom(string name, bool whiteboard, bool monitor)
        {
            string bitWhiteboard = "0";
            string bitMonitor = "0";

            if(whiteboard)
            {
                bitWhiteboard = "1";
            }
            if(monitor)
            {
                bitMonitor = "1";
            }

            String command = string.Format(
                "insert into GroupRoom (name, whiteboard, monitor) VALUES ('{0}', {1}, {2});", 
                name, bitWhiteboard, bitMonitor);
            return dbCon.ExecuteStringPut(command);
        }
        public List<ClassRoom> GetClassRoomByAttributes(bool whiteboard, bool monitor, bool projector)
        {
            String exString = ("SELECT id FROM ClassRoom WHERE(1=1");
            if(whiteboard)
            {
                exString = exString + " AND whiteboard = 1";
            }

            if (monitor)
            {
                exString = exString + " AND monitor = 1";
            }

            if (projector)
            {
                exString = exString + " AND projector = 1";
            }
            exString = exString + ");";

            SqlDataReader rs = dbCon.ExecuteStringGet(exString);

            int classRoomId;
            List<ClassRoom> classRoomList = new List<ClassRoom>();
            while (rs.Read())
            {
                classRoomId = (int)rs.GetValue(0);
                ClassRoom tempClassRoom = new ClassRoom(classRoomId);

                classRoomList.Add(tempClassRoom);
            }
            return classRoomList;
        }

        public bool RequestClassRoom(int groupId, int groupSize, bool whiteboard, bool monitor, bool projector)
        {
            int whiteboardInt = 0, monitorInt = 0, projectorInt = 0;

            if(whiteboard)
            {
                whiteboardInt = 1;
            }

            if (monitor)
            {
                monitorInt = 1;
            }

            if (projector)
            {
                projectorInt = 1;
            }

            string exString = string.Format("INSERT INTO ClassRoomWaitingList(groupId, size, whiteboard, monitor, projector) VALUES ({0}, {1}, {2}, {3}, {4});", groupId, groupSize, whiteboardInt, monitorInt, projectorInt);
            return dbCon.ExecuteStringPut(exString);
        }

        //takes boolbits from DB and creates a binary code to int... Denne metode skal bruges når DB-table 
        // over classRoomWaitingList skal omskrives til objekter. Objektet skal dannes som et 
        //  RequestClassroom med denne property requestCode.
        public List<RequestClassroom> GetAllRequests(int id)
        {
            SqlDataReader rs = dbCon.ExecuteStringGet("SELECT * FROM ClassRoomWaitinglist");

            int tempId = 0;
            int tempGroupId = 0;
            int tempSize = 0;
            int requestCode = 0;
            bool bitWhiteboard = false;
            bool bitMonitor = false;
            bool bitProjector = false;
            List<RequestClassroom> stillNotFulfilled = new List<RequestClassroom>();

            if (rs.HasRows)
            {
                while (rs.Read())
                {
                    tempId = (int) rs.GetValue(0);
                    tempGroupId = (int) rs.GetValue(1);
                    tempSize = (int) rs.GetValue(2);
                    bitWhiteboard = (bool) rs.GetValue(3);
                    bitMonitor = (bool)rs.GetValue(4);
                    bitProjector = (bool)rs.GetValue(5);

                    requestCode = CreateBinaryCode(bitWhiteboard, bitMonitor, bitProjector);

                    RequestClassroom requestClassroom = new RequestClassroom(tempGroupId, tempSize, requestCode);
                    stillNotFulfilled.Add(requestClassroom);
                   
                }


            }
            return stillNotFulfilled;

        }

        public int CreateBinaryCode(bool whiteboard, bool monitor, bool projector)
        {
            int requestCode = 0;

            if (whiteboard)
            {
               requestCode += 4;
               if (monitor)
                {
                    requestCode += 2;
                    if (projector)
                    {
                        requestCode += 1;
                    }
                }
            }
            return requestCode;
    
        }
        // Her laves liste LessThanThree med klasselokaler hentet fra DB. Denne liste bruges til 
        // algoritmesammenligningen. LessThanThree refererer til at objekterne kun skal blive på
        // listen så længe groupCount er under 3.       
        public List<ClassRoom> LessThanThree()
        {
            SqlDataReader rs = dbCon.ExecuteStringGet("SELECT * FROM ClassRoom");

            int tempId = 0;
            string tempName = null;
            int tempSize = 0;
            bool bitWhiteboard = false;
            bool bitMonitor = false;
            bool bitProjector = false;
            int requestMatch = 0;
            int groupCount = 0;
            int spaceLeft = 0;
            List<ClassRoom> LessThanThree = new List<ClassRoom>();

            if (rs.HasRows)
            {
                while (rs.Read())
                {
                    tempId = (int)rs.GetValue(0);
                    tempName = rs.GetString(1);
                    tempSize = (int)rs.GetValue(2);
                    bitWhiteboard = (bool)rs.GetValue(3);
                    bitMonitor = (bool)rs.GetValue(4);
                    bitProjector = (bool)rs.GetValue(5);

                    requestMatch = CreateBinaryCode(bitWhiteboard, bitMonitor, bitProjector);

                    ClassRoom classroom = new ClassRoom(tempId, tempSize, tempName, requestMatch, spaceLeft, groupCount);
                    LessThanThree.Add(classroom);

                }
            }
            return LessThanThree;
        }

    }
    
}
