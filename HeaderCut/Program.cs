using System;
using System.IO;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            int bytesToRemove = 0;
            int skip = 0;
            byte[] pattern = null;
            // 実行ファイルのフルパスを取得
            string executablePath = Assembly.GetExecutingAssembly().Location;

            // ディレクトリのパスを取得
            string directoryPath = Path.GetDirectoryName(executablePath);
            string folderName = "Output";
            string combinedPath = Path.Combine(directoryPath, folderName);
            try
            {
                // ディレクトリが存在しない場合に作成
                if (!Directory.Exists(combinedPath))
                {
                    Directory.CreateDirectory(combinedPath);
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"フォルダ作成時にエラーが発生しました: {ex.Message}");
            }

            //ここからバイナリの処理分岐
            Console.WriteLine("数字を入力してください:");
            Console.WriteLine("1:指定したバイト数ヘッダーをカット 2:指定したバイナリの前までヘッダーをカット 3:指定したバイナリの前までヘッダーをカット(指定回数分検索をスキップ)");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int number))
            {

                //1:指定したバイト数ヘッダーをカット
                if (number == 1)
                {
                    bool isValid = false;
                    while (!isValid)
                    {
                        Console.WriteLine("数値を入力してください。(1バイトなら1、10バイトなら10):");
                        string input_2 = Console.ReadLine();
                        if (int.TryParse(input_2, out int number_2))
                        {
                            bytesToRemove = number_2;
                            isValid = true;
                        }
                        else
                        {
                            Console.WriteLine("無効な数値です。再度入力してください。");
                        }
                    }

                    foreach (var filePath in args)
                    {
                        if (File.Exists(filePath))
                        {
                            Console.WriteLine($"ファイルが読み込まれました: {filePath}");
                            // 必要に応じてファイルの内容を処理する
                            string content = File.ReadAllText(filePath);
                            string fileName = Path.GetFileName(filePath);
                            string inputFilePath = filePath;
                            string outputFilePath = Path.Combine(combinedPath, fileName);
                            try
                            {
                                using (FileStream inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
                                {
                                    using (FileStream outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        // バイト数がファイルサイズを超えていないか確認
                                        if (bytesToRemove > inputStream.Length)
                                        {
                                            Console.WriteLine("削除するバイト数がファイルサイズを超えています。");
                                            return;
                                        }

                                        // 削除後のデータをコピー
                                        inputStream.Seek(bytesToRemove, SeekOrigin.Begin); // 削除する部分をスキップ
                                        byte[] buffer = new byte[8192]; // バッファサイズ
                                        int bytesRead;

                                        while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            outputStream.Write(buffer, 0, bytesRead);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"エラーが発生しました: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"指定されたパスは無効です: {filePath}");
                        }
                    }
                }
                else if (number == 2 || number == 3)
                {
                    if (number == 3)
                    {
                        bool isValid = false;
                        while (!isValid)
                        {
                            Console.WriteLine("検索スキップする回数を入力してください:");
                            string input_3 = Console.ReadLine();
                            if (int.TryParse(input_3, out int number_3))
                            {
                                skip = number_3;
                                isValid = true;
                            }
                            else
                            {
                                Console.WriteLine("無効な数値です。再度入力してください。");
                            }
                        }
                    }
                    Console.WriteLine("16進形式でデータを入力してください（例: 89 50 4E 47）:");
                    string input_4 = Console.ReadLine();
                    try
                    {
                        // 入力文字列をバイト配列に変換
                        pattern = ParseHexStringToByteArray(input_4);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"エラーが発生しました: {ex.Message}");
                    }
                    foreach (var filePath in args)
                    {
                        if (File.Exists(filePath))
                        {
                            Console.WriteLine($"ファイルが読み込まれました: {filePath}");
                            // 必要に応じてファイルの内容を処理する
                            string content = File.ReadAllText(filePath);
                            string fileName = Path.GetFileName(filePath);
                            string inputFilePath = filePath;
                            string outputFilePath = Path.Combine(combinedPath, fileName);

                            using (FileStream inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
                            using (FileStream outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                            {
                                long position = FindPatternPosition(inputStream, pattern, skip);
                                if (position >= 0)
                                {
                                    Console.WriteLine($"パターンの位置: {position}");
                                    // 指定位置からデータを出力ファイルに書き込む
                                    //inputStream.Seek(position + pattern.Length, SeekOrigin.Begin); // パターンの直後に移動
                                    inputStream.Seek(position, SeekOrigin.Begin); // パターンの直後に移動
                                    inputStream.CopyTo(outputStream);
                                }
                                else
                                {
                                    Console.WriteLine("指定したパターンが見つかりませんでした。");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"指定されたパスは無効です: {filePath}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("有効な数字を入力してください。");
                }
            }
            else
            {
                Console.WriteLine("有効な数字を入力してください。");
            }
        }
        else
        {
            Console.WriteLine("ドラッグ＆ドロップされたファイルがありません。");
        }
        Console.WriteLine("処理が完了しました。エンターキーを押してください。");
        Console.ReadLine();
    }
    static long FindPatternPosition(FileStream stream, byte[] pattern, int skipnum)
    {
        byte[] buffer = new byte[8192]; // 読み取りバッファ
        int bytesRead;
        long currentPosition = 0; // 現在の読み取り位置

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i <= bytesRead - pattern.Length; i++)
            {
                bool isMatch = true;

                // パターンを比較
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (buffer[i + j] != pattern[j])
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    if (skipnum == 0)
                    {
                        return currentPosition + i; // パターンの開始位置を返す
                    }
                    else
                    {
                        skipnum = skipnum - 1;
                    }
                }
            }

            // 次のバッファに進む
            currentPosition += bytesRead;
            stream.Position = currentPosition - (pattern.Length - 1); // 重複部分を考慮
        }

        return -1; // パターンが見つからなかった場合
    }
    static byte[] ParseHexStringToByteArray(string hexString)
    {
        // 空白で分割
        string[] hexValues = hexString.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        // 16進値をバイト配列に変換
        return hexValues.Select(hex => Convert.ToByte(hex, 16)).ToArray();
    }
}