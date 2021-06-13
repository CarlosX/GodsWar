using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Constants
{
    public enum ResponseCodes
    {
         ID_NO_REGISTER,    //* 0: el ID no está registrado; 
         SESSION_SUCCESS,   //* 1: el inicio de sesión es exitoso; 
         SESSION_REPEAT,    //* 2: inicio de sesión repetido; 
         ERROR_PASSWORD,    //* 3: error de contraseña; 
         ERROR_VERSION      //* 4: error de versión
    }
}