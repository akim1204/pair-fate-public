using Godot;
using System;

public class Logger
{
    /// <summary>
    /// Singleton Logger instance.
    /// </summary>
    private static Logger _instance;

    public static Logger Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Logger();
            }

            return _instance;
        }

        private set
        {
            _instance = value;
        }
    }

    /// <summary>
    /// Current severity level.
    /// </summary>
    public enum LOG_LEVELS
    {
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL,
    }

    public LOG_LEVELS log_level = LOG_LEVELS.DEBUG;

    /// <summary>
    /// Display a single log message.
    /// </summary>
    /// <param name="level">Severity level at which to display message</param>
    /// <param name="msg">The message to be displayed</param>
    public void Log(LOG_LEVELS level, string msg)
    {
        if (log_level <= level)
        {
            GD.Print(msg);
        }
    }
}
