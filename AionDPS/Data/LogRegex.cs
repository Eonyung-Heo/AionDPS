using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AionDPS
{
    public struct Analyzed
    {
        public DateTime loggedTime;
        public string userName;
        public string skillName;
        public string hittedObjectName;
        public int damage;
        public bool isCritical;
        public bool isCastSpd;
        public bool isAttackSpd;
        public bool rage;
        public bool isSpiritWall;
        public string transform;
        public int rageDamage;
        

    }

    class LogRegex : SingletonBase<LogRegex>
    {
        private static string loggedTimestamp = @"(?<loggedTime>[0-9]{4}.[0-9]{2}.[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2}) : ";
        public static Analyzed getLogResult(string log, string hittedObjectName = @"[가-힣0-9A-Za-z\s]+")
        {

            int serverEndMin = 0; 
            int serverStartMin = 0;
            int serverStartHour = 0;

            /*
            불의 정령이 명령: 수호의 장벽 IV 불을 사용해 순진이 반사 상태가 됐습니다. 
            불의 정령이 명령: 수호의 장벽 IV 불을 사용해 순진이 물리 공격력 강화 상태가 됐습니다. 
             */


            Regex rx = new Regex(loggedTimestamp + @"(?<isCritical>치명타! )?((?<userName>[가-힣A-Za-z\s]+)(가|이) )?((?<skillName>[가-힣\s]+ (I)?(I)?(V)?(I)?(I)?)을 사용해 )?" + $@"(?<hittedObjectName>{hittedObjectName})" + @"(에게|이) (?<damage>[0-9,]+)의 (치명적인 )?대미지를 (받고|받았|줬습|주고)");
            Regex rx2 = new Regex(loggedTimestamp + @"((?<userName>[가-힣A-Za-z]+)(가|이) )?(사용한 )?신속의 주문 I(을 사용해|의 영향으로) ((?<targetName>[가-힣A-Za-z]+)의 )?시전속도(가|를) (변동됐습니다|변경했습니다)");
            Regex rx6 = new Regex(loggedTimestamp + @"((?<userName>[가-힣A-Za-z]+)(가|이) )?(사용한 )?질풍의 주문 I(을 사용해|의 영향으로) ((?<targetName>[가-힣A-Za-z]+)(가|이) )?이동속도 강화 (상태|효과)가 (발생했습니다|됐습니다)");
            Regex rx7 = new Regex(loggedTimestamp + @"(불의 정령이 명령:) 수호의 장벽 (I)?(I)?(V)?(I)?(I)? 불을 사용해 ((?<targetName>[가-힣A-Za-z]+)(가|이) )?(물리 공격력 강화) (상태|효과)가 (발생했습니다|됐습니다)");

            Regex rx3 = new Regex(loggedTimestamp + $@"{hittedObjectName}이 (사용한 )?신장의 (격노|분노)(를 사용해|의 영향으로) ((?<targetName>[가-힣A-Za-z]+)에게 )?(?<damage>[0-9,]+)의 대미지를 (줬습니다|받았습니다).");
            Regex rx4 = new Regex(loggedTimestamp + @"(?<isCritical>치명타! )?((?<userName>[가-힣A-Za-z\s]+)(가|이) )?((심연의 (폭풍|기운|반사막|파동|해일)+ (I)?(I)?(V)?(I)?(I)?)을 사용해 )" + $@"(?<hittedObjectName>{hittedObjectName})" + @"(에게|이) (?<damage>[0-9,]+)의 (치명적인 )?대미지를 (받고|받았|줬습|주고)");
            Regex rx5 = new Regex(loggedTimestamp + @"((?<userName>[가-힣A-Za-z\s]+)(가|이) )?(변신: 수호신장 (I)?(I)?(V)?(I)?(I)?을 사용해 아바타 (천족|마족)으로 변신했습니다).");

            Analyzed analyzed = new Analyzed();


            if ((Main.form.fortressComboBox.Text == "어비스"))
                serverStartHour = 19;
            else
                serverStartHour = 22;

            if (Main.form.serverComboBox.Text == "트리니엘" || Main.form.serverComboBox.Text == "시엘" || Main.form.serverComboBox.Text == "어비스")
            {
                serverStartHour = serverStartHour - 1;
                serverStartMin = 55;
                serverEndMin = 10;
            }
            else if(Main.form.serverComboBox.Text == "이스라펠")
            {
                serverStartMin = 0;
                serverEndMin = 15;
            }
            else if(Main.form.serverComboBox.Text == "네자칸" || Main.form.serverComboBox.Text == "크로")
            {
                serverStartMin = 5;
                serverEndMin = 20;
            }
            else if(Main.form.serverComboBox.Text == "지켈")
            {
                serverStartMin = 10;
                serverEndMin = 25;
            }
            else if(Main.form.serverComboBox.Text == "바이젤" || Main.form.serverComboBox.Text == "에레슈렌타")
            {
                serverStartMin = 15;
                serverEndMin = 30;
            }


            
            
            if (rx3.IsMatch(log) && Main.form.checkBox3.Checked)
            {
                Match matched = rx3.Match(log);

                string userName = matched.Groups["targetName"].Value;

                string damage = matched.Groups["damage"].Value;
                analyzed.userName = userName == "" ? Main.form.textBox1.Text : userName;
                analyzed.rage = true;
                analyzed.rageDamage = int.Parse(damage, NumberStyles.AllowThousands);
                analyzed.transform = "N";

                analyzed.loggedTime = DateTime.ParseExact(matched.Groups["loggedTime"].Value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            else if (rx2.IsMatch(log) && Main.form.checkBox2.Checked)
            {
                Match matched = rx2.Match(log);
                string userName = matched.Groups["userName"].Value;
                string targetName = matched.Groups["targetName"].Value;

                analyzed.loggedTime = DateTime.ParseExact(matched.Groups["loggedTime"].Value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                int loggedHour = analyzed.loggedTime.Hour;
                int loggedMin = analyzed.loggedTime.Minute;

                if (targetName == "")
                {
                    targetName = log.Contains("사용한") ? Main.form.textBox1.Text : "";
                }

                if ((Main.form.fortressComboBox.Text == "어비스" && loggedHour == 19 || (loggedHour == serverStartHour && (loggedMin > serverStartMin && loggedMin < serverEndMin) )) ||
                    (Main.form.fortressComboBox.Text != "어비스" && loggedHour == 22 || (loggedHour == serverStartHour && (loggedMin > serverStartMin && loggedMin < serverEndMin) )))
                {
                    analyzed.userName = userName;
                    analyzed.hittedObjectName = targetName;
                    analyzed.isCastSpd = true;
                }
                else
                {
                    analyzed.userName = null;
                    analyzed.hittedObjectName = null;
                    analyzed.isCastSpd = false;
                }
                analyzed.rage = false;
                analyzed.transform = "N";
                return analyzed;
            }
            else if (rx6.IsMatch(log) && Main.form.checkBox2.Checked)
            {
                Match matched = rx6.Match(log);
                string userName = matched.Groups["userName"].Value;
                string targetName = matched.Groups["targetName"].Value;

                analyzed.loggedTime = DateTime.ParseExact(matched.Groups["loggedTime"].Value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                int loggedHour = analyzed.loggedTime.Hour;
                int loggedMin = analyzed.loggedTime.Minute;

                if (targetName == "")
                {
                    targetName = log.Contains("사용한") ? Main.form.textBox1.Text : "";
                }


                if ((Main.form.fortressComboBox.Text == "어비스" && loggedHour == 19 || (loggedHour == serverStartHour && (loggedMin > serverStartMin && loggedMin < serverEndMin) )) ||
                    (Main.form.fortressComboBox.Text != "어비스" && loggedHour == 22 || (loggedHour == serverStartHour && (loggedMin > serverStartMin && loggedMin < serverEndMin))))
                {
                    analyzed.userName = userName;
                    analyzed.hittedObjectName = targetName;
                    analyzed.isAttackSpd = true;
                }
                else
                {
                    analyzed.userName = null;
                    analyzed.hittedObjectName = null;
                    analyzed.isAttackSpd = false;
                }

                analyzed.rage = false;
                analyzed.transform = "N";
                return analyzed;
            }
            else if(rx5.IsMatch(log))
            {
                Match matched = rx5.Match(log);

                string userName = matched.Groups["userName"].Value;

                analyzed.loggedTime = DateTime.ParseExact(matched.Groups["loggedTime"].Value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                int loggedHour = analyzed.loggedTime.Hour;
                int loggedMin = analyzed.loggedTime.Minute;
                
                if ((Main.form.fortressComboBox.Text == "어비스" && loggedHour == 19 || (loggedHour == serverStartHour && (loggedMin > serverStartMin && loggedMin < serverEndMin) )) ||
                    (Main.form.fortressComboBox.Text != "어비스" && loggedHour == 22 || (loggedHour == serverStartHour && (loggedMin > serverStartMin && loggedMin < serverEndMin))))
                {
                    analyzed.userName = userName;
                    analyzed.transform = "Y";
                }
                else
                {
                    analyzed.userName = null;
                    analyzed.transform = "N";
                }
            }
            else if(rx7.IsMatch(log))
            {
                Match matched = rx7.Match(log);
                string userName = matched.Groups["userName"].Value;
                string targetName = matched.Groups["targetName"].Value;

                analyzed.loggedTime = DateTime.ParseExact(matched.Groups["loggedTime"].Value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                int loggedHour = analyzed.loggedTime.Hour;
                int loggedMin = analyzed.loggedTime.Minute;

                if (targetName == "")
                {
                    targetName = log.Contains("사용한") ? Main.form.textBox1.Text : "";
                }


                if ((Main.form.fortressComboBox.Text == "어비스" && loggedHour == 19 || (loggedHour == serverStartHour && (loggedMin > serverStartMin && loggedMin < serverEndMin))) ||
                    (Main.form.fortressComboBox.Text != "어비스" && loggedHour == 22 || (loggedHour == serverStartHour && (loggedMin > serverStartMin && loggedMin < serverEndMin))))
                {
                    analyzed.userName = targetName;
                    analyzed.hittedObjectName = targetName;
                    analyzed.isSpiritWall = true;
                }
                else
                {
                    analyzed.userName = null;
                    analyzed.hittedObjectName = null;
                    analyzed.isSpiritWall = false;
                }

                analyzed.rage = false;
                analyzed.transform = "N";
                return analyzed;
            }
            
            else if (rx.IsMatch(log))
            {
                Match matched = rx.Match(log);
                analyzed.transform = "N";

                if (rx4.IsMatch(log))
                    analyzed.transform = "Y";

                string userName = matched.Groups["userName"].Value;
                string skillName = matched.Groups["skillName"].Value;
                if (userName == "")
                    userName = Main.form.textBox1.Text;
                else if (userName.IndexOf("의 정령") != -1 || userName.IndexOf("의 기운") != -1)
                    userName = null;
                else if (userName.IndexOf(' ') != -1 && skillName.IndexOf(userName) != -1)
                    userName = null;
                else if (userName == "빙판" && skillName.StartsWith("빙판") && skillName.EndsWith("효과"))
                    userName = null;
                else if ((userName.IndexOf(" ")) != -1)
                    userName = null;

                analyzed.loggedTime = DateTime.ParseExact(matched.Groups["loggedTime"].Value, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                analyzed.userName = userName;
                analyzed.skillName = matched.Groups["skillName"].Value.Length > 0 ? matched.Groups["skillName"].Value : "";
                analyzed.hittedObjectName = matched.Groups["hittedObjectName"].Value;
                analyzed.damage = int.Parse(matched.Groups["damage"].Value, NumberStyles.AllowThousands);
                analyzed.isCritical = matched.Groups["isCritical"].Value.Contains("치명타!");
                analyzed.rage = false;

            }

            return analyzed;
        }

        public static string getUserName(string log)
        {
            return getLogResult(log).userName;
        }
    }

}
