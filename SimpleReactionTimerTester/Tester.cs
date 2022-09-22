using System;

namespace SimpleReactionMachine
{
    class Tester
    {
        private static IController controller;
        private static IGui gui;
        private static string displayText;
        private static int randomDelay;
        private static int passed = 0;

        static void Main(string[] args)
        {
            // run simple test
            SimpleTest();
            Console.WriteLine("\n=====================================\nSummary: {0} tests passed out of 46", passed);
            Console.ReadKey();
        }

        /*
         * Runs through three sets of games, testing different combinations of 
         * entry and exit code between all 6 states in the machine.
         * 
         * Tests:
         * - No Coin State: tests insert coin + go/stop inactive + no timeout
         * - Coin State: tests coin timeout (10s) + go/stop pressed to proceed
         * - RandomDelay State: tested by randomDelay variable + go/stop aborts game
         * - GamePlay State: tests timeout (2s) + go/stop pressed
         * - Final Score State: tests timeout (3s) + go/stop to skip
         * - Average Score State: tests timeout (5s) + go/stop to end game
         */
        private static void SimpleTest()
        {
            //Construct a ReactionController
            controller = new SimpleReactionController();
            gui = new DummyGui();

            //Connect them to each other
            gui.Connect(controller);
            controller.Connect(gui, new RndGenerator());

            //Reset the components()
            gui.Init();

            //Test the EnhancedReactionController

            // ==================== GAME ONE ==================== //
            // IDLE //
            DoReset('A', controller, "Insert coin");
            DoGoStop('B', controller, "Insert coin");
            DoTicks('C', controller, 1, "Insert coin");

            // Test inserting coin //
            DoInsertCoin('D', controller, "Press GO!");

            // Test coin timeout (10s) //
            DoTicks('E', controller, 1000, "Insert Coin");

            // Proceed with gameplay //
            randomDelay = 142;
            DoInsertCoin('F', controller, "Press GO!");
            DoGoStop('G', controller, "Wait...");
            DoTicks('H', controller, randomDelay + 34, "0.34");
            DoGoStop('I', controller, "0.34");

            // Skips final score display -> should proceed to next game /
            DoGoStop('J', controller, "Press GO!");

            // ==================== GAME TWO ==================== //

            // Proceed with game play //
            randomDelay = 121;
            DoGoStop('K', controller, "Wait...");

            // Test InsertCoin button inactive //
            DoInsertCoin('L', controller, "Wait...");
            DoTicks('M', controller, randomDelay + 58, "0.58");
            DoGoStop('N', controller, "0.58");

            // Test final score display timeout (3s) -> should proceed to next game //
            DoTicks('O', controller, 300, "Press GO!");

            // ==================== GAME THREE ==================== //

            // Proceed with game play //
            randomDelay = 231;
            DoGoStop('P', controller, "Wait...");
            DoTicks('Q', controller, randomDelay + 30, "0.30");
            DoGoStop('R', controller, "0.30");
            DoGoStop('S', controller, "Average = 0.41");

            // Skips average score display -> should end game //
            DoGoStop('T', controller, "Insert Coin");

            // ==================== GAME ONE ==================== //

            // Proceed with game play //
            DoInsertCoin('U', controller, "Press GO!");
            randomDelay = 205;
            DoGoStop('V', controller, "Wait...");
            DoTicks('W', controller, randomDelay + 145, "1.45");
            DoGoStop('X', controller, "1.45");

            // Skips final score display -> should proceed to next game //
            DoGoStop('Y', controller, "Press GO!");

            // ==================== GAME TWO ==================== //

            // Proceed with game play //
            DoInsertCoin('Z', controller, "Press GO!");
            randomDelay = 175;
            DoGoStop('a', controller, "Wait...");
            DoTicks('b', controller, randomDelay + 200, "2.00");

            // Test final score display timeout -> should proceed to next game //
            DoTicks('c', controller, 300, "Press GO!");

            // ==================== GAME THREE ==================== //

            // Proceed with game play //
            DoInsertCoin('d', controller, "Press GO!");
            randomDelay = 139;
            DoGoStop('e', controller, "Wait...");

            // Test go/stop during Wait... period -> should abort gameplay //
            DoGoStop('f', controller, "Insert Coin");

            // ==================== GAME ONE ==================== //

            // Proceed with gameplay //
            randomDelay = 123;
            DoInsertCoin('g', controller, "Press GO!");
            DoGoStop('h', controller, "Wait...");
            DoTicks('i', controller, randomDelay + 168, "1.68");
            DoGoStop('j', controller, "1.68");

            // Skips final score display -> should proceed to next game //
            DoGoStop('k', controller, "Press GO!");

            // ==================== GAME TWO ==================== //

            // Proceed with gameplay //
            randomDelay = 175;
            DoGoStop('l', controller, "Wait...");
            DoTicks('m', controller, randomDelay + 45, "0.45");
            DoGoStop('n', controller, "0.45");

            // Skip final score display -> should proceed to next game //
            DoGoStop('o', controller, "Press GO!");

            // ==================== GAME THREE ==================== //

            // Proceed with game play //
            randomDelay = 201;
            DoGoStop('p', controller, "Wait...");
            DoTicks('q', controller, randomDelay + 102, "1.02");
            DoGoStop('r', controller, "1.02");
            DoGoStop('s', controller, "Average = 1.05");

            // Test average score display timeout (5s) -> should end game //
            DoTicks('t', controller, 500, "Insert Coin");
        }

        private static void DoReset(char ch, IController controller, string msg)
        {
            try
            {
                controller.Init();
                GetMessage(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void DoGoStop(char ch, IController controller, string msg)
        {
            try
            {
                controller.GoStopPressed();
                GetMessage(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void DoInsertCoin(char ch, IController controller, string msg)
        {
            try
            {
                controller.CoinInserted();
                GetMessage(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void DoTicks(char ch, IController controller, int n, string msg)
        {
            try
            {
                for (int t = 0; t < n; t++) controller.Tick();
                GetMessage(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void GetMessage(char ch, string msg)
        {
            if (msg.ToLower() == displayText.ToLower())
            {
                Console.WriteLine("test {0}: passed successfully", ch);
                passed++;
            }
            else
                Console.WriteLine("test {0}: failed with message ( expected {1} | received {2})", ch, msg, displayText);
        }

        private class DummyGui : IGui
        {

            private IController controller;

            public void Connect(IController controller)
            {
                this.controller = controller;
            }

            public void Init()
            {
                displayText = "?reset?";
            }

            public void SetDisplay(string msg)
            {
                displayText = msg;
            }
        }

        private class RndGenerator : IRandom
        {
            public int GetRandom(int from, int to)
            {
                return randomDelay;
            }
        }

    }

}
