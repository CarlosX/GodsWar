using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.Networking
{
    class Base2
    {
    }


    /*
    //Message header
    struct  MsgHead
    {
	    USHORT	usSize;
	    USHORT	usType;
    };

    //Login message\
    ( Client  ----> LoginServer )
    struct  MSG_LOGIN
    {
	    MsgHead  Head;
	    char     Name[MAX_NAME];
	    char     cPassWord[MAX_NAME];
	    long     fVersion;
    };

    //Login back \
    ( LoginServer  ----> Client )
    struct  MSG_LOGIN_RETURN_INFO
    {
	    MsgHead 	 Head;
	    BYTE    	 ucInfo;                   //0: ID is not registered; 1: successful login; 2: repeated login; 3: wrong password; 4: wrong version
    };


    //Register game server\
    ( GameServer ----> LoginServer ) \
    ( LoginServer----> Client      )
    struct MSG_GAMESERVER_INFO 
    {
	    MsgHead  Head;
	    char     cIP[MAX_NAME];
	    UINT     uiPort;
	    BYTE     cID;                         //Server id
	    char     ServerName[MAX_NAME];        //Game server name
	    BYTE     cState;                      //0: Start; 1: Idle; 2: Busy; 3: Full; 4: Close
    };*/
}
