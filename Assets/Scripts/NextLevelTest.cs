using UnityEngine;
using UnityEngine.SceneManagement; // нужно для работы со сценами

public class NextLevelTest : MonoBehaviour
{
    // Этот метод вызывается кнопкой Next Level
    public void OnClickTest()
    {
        Debug.Log("NextLevelTest: кнопка нажата!");

        // Получаем текущую сцену
        Scene current = SceneManager.GetActiveScene();

        // Вариант 1: перезагрузить этот же уровень (как у тебя было)
        // SceneManager.LoadScene(current.buildIndex);

        // Вариант 2: перейти на следующий уровень по номеру в Build Settings
        int nextIndex = current.buildIndex + 1;

        // Если следующий уровень есть – загружаем его,
        // если нет – просто перезагружаем текущий (чтобы не было ошибки)
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            SceneManager.LoadScene(current.buildIndex);
        }
    }
}