namespace Test;

internal static class TestExceptionGenerator
{
    public static AggregateException? Generate()
    {
        try
        {
            agregate_method();
            return null;
        }
        catch (AggregateException ex)
        {
            return ex;
        }
    }

    static void method1()
    {
        throw new ArgumentException("Exception 1\nmessage тест");
    }

    static void method2()
    {
        method1();
    }

    static void method3()
    {
        try
        {
            method2();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Exception 2\nmessage", ex);
        }
    }

    static void method4()
    {
        method3();
    }

    static void method5()
    {
        try
        {
            method4();
        }
        catch (Exception ex)
        {
            throw new ArgumentOutOfRangeException("Exception 3\nmessage", ex);
        }
    }

    static void method6()
    {
        method5();
    }

    static void other_method1()
    {
        throw new ArgumentException("Exception 11\nmessage");
    }

    static void other_method2()
    {
        other_method1();
    }

    static void other_method3()
    {
        try
        {
            other_method2();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Exception 22\nmessage", ex);
        }
    }

    static void other_method4()
    {
        other_method3();
    }

    static void other_method5()
    {
        try
        {
            other_method4();
        }
        catch (Exception ex)
        {
            throw new ArgumentOutOfRangeException("Exception 33\nmessage", ex);
        }
    }


    static void other_method6()
    {
        other_method5();
    }

    static void agregate_method()
    {
        Exception? ex1 = null;
        Exception? ex2 = null;
        try
        {
            method6();
        }
        catch (Exception ex)
        {
            ex1 = ex;
        }

        try
        {
            other_method6();
        }
        catch (Exception ex)
        {
            ex2 = ex;
        }

        throw new AggregateException("Agregate ex\nmessage", ex1, ex2);
    }
}
