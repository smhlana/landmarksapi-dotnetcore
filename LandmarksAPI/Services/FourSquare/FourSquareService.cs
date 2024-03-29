﻿using FourSquare.SharpSquare.Core;
using FourSquare.SharpSquare.Entities;
using System;
using System.Collections.Generic;

namespace LandmarksAPI.Services
{
	public class FourSquareService : IFourSquareService
	{
		private SharpSquare _sharpSquareClient;

		public FourSquareService(string clientId, string clientSecret)
		{
			_sharpSquareClient = new SharpSquare(clientId, clientSecret);
		}

		public List<Venue> SearchVenues(Dictionary<string, string> parameters)
		{
			try
			{
				return _sharpSquareClient.SearchVenues(parameters);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}
