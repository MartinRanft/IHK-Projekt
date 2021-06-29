using System;

using Wareneingang;
using Wareneingang.Data.com_class;
using Wareneingang.Funktion.Login;

using Xunit;

namespace WareneingangTest
{
    public class UnitTest1
    {
        [Theory]
        [InlineData("BATestC#user", "baTestuserLogin1", 0)]
        public async void loginTest(String username, String password, int company)
        {
            loginCom login = new loginCom();
            var User = await login.authcheck(username, password, company);
            int ID = User.data.user_id;
            Console.WriteLine(User.data.user_id);
            Assert.Equal(2233, ID);
        }
    }
}