using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefineServerUtility
{
    // For DataBase
    public enum eDBFunction
    {
        INSERT = 1,
        DELETE,
        IDCHECK,
        LOGINCHECK,
        SETNICKNAME,
        QUIT,

        WAIT
    }
    class DefineDBValue
    {
        public const string _baselocalIP = "localhost";
        public const int _port = 3306;
        public const string _insertLoginInfo = "INSERT INTO memberinfo(UUID, ID, PW, NICKNAME) VALUES({0}, '{1}', '{2}', '');";
        public const string _idCheck = "SELECT ID from memberinfo;";
        public const string _nicknameCheck = "SELECT NICKNAME from memberinfo;";
        public const string _loginCheck = "SELECT * from memberinfo;";
        public const string _setNickName = "update memberinfo set NICKNAME = '{1}' where ID = '{0}';";
    }
}
