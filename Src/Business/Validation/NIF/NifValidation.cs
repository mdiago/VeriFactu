﻿using System.Collections.Generic;

namespace VeriFactu.Business.Validation.NIF
{

    /// <summary>
    /// Valida si un NIF está identificado en la AEAT.
    /// </summary>
    public class NifValidation
    {

        string _Nif;
        string Name;

        public NifValidation(string name, string nif) 
        { 
        
        }

        /// <summary>
        /// Ejecuta las validaciones del obejeto de negocio.
        /// </summary>
        public virtual List<string> GetErrors() 
        {

            return new List<string>();

        }

    }
}