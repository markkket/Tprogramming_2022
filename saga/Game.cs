namespace CourseApp
{
    using System;
    using System.Collections.Generic;

    public class Game
    {
        public static void Start()
        {
            int count = CountOfPlayers(); // спрашиваем у пользователя число игроков
            List<Player> participant = GetPlayersList(count); // генерируем лист игроков
            Tourney(participant); // запускаем турнир
        }

        private static void Tourney(List<Player> participant)
        {
            // играем до тех пор, пока не останется 1 игрок
            for (int i = 1; participant.Count != 1; i++)
            {
                Logger.WriteRound(i); // передаем в логгер номер раунда
                Randomize(participant); // в начале каждого раунда случайным образом перемешиваем игроков
                Match(participant); // запускаем раунд
            }

            Logger.WriteWinner(participant[0]); // передаем в логгер последнего оставщегося игрока
        }

        private static void Match(List<Player> participant)
        {
            // так как сражения происходят в парах, то обрабатывать будем до половины листа
            for (int i = 0; i < participant.Count / 2; i++)
            {
                Player[] opponents = { participant[i * 2], participant[(i * 2) + 1] }; // записываем в массив пару бойцов
                Logger.WriteFight(opponents); // передаем в логгер созданный массив
                participant[i * 2] = Duel(opponents); // записываем в первое место пары победителя
            }

            // увеличиваем i на единицу, так как при удалении элемента массив сокращается и новый победитель оказывается на только что обработанном месте листа
            for (int i = 1; i < participant.Count; i++)
            {
                participant.RemoveAt(i); // удаляем всех проигравших
            }
        }

        private static Player Duel(Player[] opponents)
        {
            // используем for а не while из-за удобства наличия счетчика
            for (int i = 0; true; i++)
            {
                string dbf_status = opponents[i % 2].DebaffStatus(); // проверяем какой дебаф действует на игрока
                Logger.WriteAction(opponents[i % 2], dbf_status); // передаем этот дебаф и игрока в логгер

                // если дебаф был с периодическим уроном, то передаем его
                if (dbf_status == "Огненная стрела")
                {
                    opponents[i % 2].GetDamage(2);
                }

                bool checkDeath = opponents[i % 2].CheckDeath(); // проверяем не умер ли первый игрок от периодического урона
                if (checkDeath)
                {
                    Logger.WriteDeath(opponents[i % 2]); // передаем в логер умершего игрока
                    opponents[(i + 1) % 2].Refresh(); // восстанавливаем второго игрока
                    return opponents[(i + 1) % 2]; // передаем его как победителя
                }

                // если дебаф на пропуск хода, соответсвенно пропускаем его
                if (dbf_status == "Заворожение")
                {
                    continue;
                }

                string playerAction = GetAction(opponents[i % 2]); // выбираем действие игрока

                // если действие не атакующие, передаем в логер игрока, совершившего действие, игрока, над которым соверешенно действие, и название действия
                if ((playerAction != "Атака") && (playerAction != "Удар возмездия"))
                {
                    Logger.WriteAction(opponents[i % 2], opponents[(i + 1) % 2], playerAction);
                    opponents[(i + 1) % 2].SetDebaff(playerAction); // записываем его в дебаф
                }

                // если действие атакующие, получаем значение урона и передаем в логер игрока, совершившего действие, игрока, над которым соверешенно действие, и урон
                else
                {
                    float damage = opponents[i % 2].Attack();
                    Logger.WriteAction(opponents[i % 2], opponents[(i + 1) % 2], playerAction, damage);
                    opponents[(i + 1) % 2].GetDamage(damage); // наносим урон
                }

                bool death = opponents[(i + 1) % 2].CheckDeath(); // проверяем не умер ли игрок после получения урона
                if (checkDeath)
                {
                    Logger.WriteDeath(opponents[(i + 1) % 2]); // передаем умершего игрока
                    opponents[i % 2].Refresh(); // восстанавливаем победителя и возвращаем его
                    return opponents[i % 2];
                }
            }
        }

        private static string GetAction(Player inputP)
        {
            Random rnd = new Random();
            int chosen = rnd.Next(0, 4); // случайным образом генерируем дейтсвие игрока
            switch (chosen)
            {
                case 0:
                // если игрок может совершить специальное действие возвращаем его название, иначе передаем команду об обычной атаке
                    if (inputP.CheckAbility())
                    {
                        return inputP.Ability();
                    }

                    return "Атака";
                default:
                    return "Атака";
            }
        }

        private static int CountOfPlayers()
        {
            // до тех пор, пока не введется корректное число, не возвращаем значение
            while (true)
            {
                Console.WriteLine("Введите число игроков:");
                int count = Convert.ToInt32(Console.ReadLine());
                if (count <= 0)
                {
                    Console.WriteLine("Введите число больше 0");
                }
                else if (count % 2 != 0)
                {
                    Console.WriteLine("Введите четное число");
                }
                else
                {
                    return count;
                }
            }
        }

        private static void Randomize(List<Player> input)
        {
            // переставляем элементы листа начиная с 0-го
            Random rnd = new Random();
            for (int i = 0; i < input.Count; i++)
            {
                int chosen = rnd.Next(i, input.Count);
                (input[i], input[chosen]) = (input[chosen], input[i]); // перестановка элементов местами
            }
        }

        private static List<Player> GetPlayersList(int count)
        {
            // создаем лист и возрващаем его заполненный вариант
            List<Player> playerList = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                playerList.Add(GenPlayer()); // добавляем игрока в лист
            }

            return playerList;
        }

        private static Player GenPlayer()
        {
            string[] names = { "Рюрик", "Вещий Олег", "Игорь Старый", "Ольга", "Святослав", "Владимир I", "Ярослав Мудрый", "Всеволод Переяславский", "Владимир Мономах", "Юрий Долгорукий", "Андрей Боголюбский", "Всеволод Большое Гнездо", "Ярослав Всеволодович", "Александр Невский", "Даниил Московский", "Иван Калита" };
            Random random = new Random();

            // рандомно выбираем здоровье, силу, класс и имя, затем возрващаем полученного игрока
            int hlth = (int)random.NextInt64(20, 101);
            int str = (int)random.NextInt64(5, 21);
            int player_class = (int)random.NextInt64(0, 3);
            switch (player_class)
            {
                case 0:
                    return new Knight(hlth, str, names[random.Next(names.Length)]); // передаем в конструктор класса здоровье, силу и элемент массива имен со случайного места
                case 1:
                    return new Mage(hlth, str, names[random.Next(names.Length)]);
                case 2:
                    return new Archer(hlth, str, names[random.Next(names.Length)]);
                default:
                    return new Knight(hlth, str, names[random.Next(names.Length)]);
            }
        }
    }
}