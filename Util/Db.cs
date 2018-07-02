/*
	This class aims to abstract away SQLite and related file operations.
*/
using Godot;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Mono.Data.Sqlite;

public class Db{
	const string DefaultFile = @"Db/save.db";
	public string file;
	IDbConnection conn;
	IDbCommand cmd;

	public Db(string file){
		conn = new SqliteConnection("URI=file:" + file);
		cmd = conn.CreateCommand();
		conn.Open();
	}

	public void PrintSettings(){
		string sql = @"
			SELECT * from settings;
		";
		cmd.CommandText = sql;
		IDataReader rdr = cmd.ExecuteReader();
		while(rdr.Read()){
			GD.Print(rdr["name"] + ":" + rdr["value"]);
		}
		rdr.Close();
	}

	public void Clear(){
		conn.Close();
	}

	public void CreateTables(){
		string sql = @"
		CREATE TABLE settings 
		(
			name VARCHAR(200) PRIMARY KEY, 
			value VARCHAR(200)
		)
		";
		cmd.CommandText = sql;
		cmd.ExecuteNonQuery();
	}

	public void InitSettings(){
		StoreSetting("volume", "1.0f");
		StoreSetting("username", "New Player");
		StoreSetting("first_login", DateTime.Today.ToString("MM/dd/yyyy"));
		StoreSetting("mouse_sensitivity_x", "1.0f");
		StoreSetting("mouse_sensitivity_y", "1.0f");
	}

	public void StoreSetting(string name, string val){
		string sql = @"
			INSERT OR IGNORE INTO settings(name, value)
			VALUES (@name, @value);
			UPDATE settings
			SET value = @value 
			WHERE name = @name;
		";
		cmd.CommandText = sql;
		cmd.Parameters.Add(new SqliteParameter ("@name", name));
		cmd.Parameters.Add(new SqliteParameter ("@value", val));
		cmd.ExecuteNonQuery();
	}

	public string SelectSetting(string name){
		string sql = @"
			SELECT value FROM settings
			WHERE name = @name
		";
		cmd.CommandText = sql;
		cmd.Parameters.Add(new SqliteParameter ("@name", name));
		IDataReader rdr = cmd.ExecuteReader();
		string ret = "";
		if(rdr.Read()){
			ret = rdr["value"] as string;
		}
		rdr.Close();
		return ret;
	}





	// Static methods

	public static Db Init(){
		if(System.IO.File.Exists(DefaultFile)){
			return new Db(DefaultFile);
		}
		CreateFile(DefaultFile);
		Db db = new Db(DefaultFile);
		db.CreateTables();
		db.InitSettings();
		db.PrintSettings();
		return db;
	}
	
	public static void CreateFile(string file){
		SQLiteConnection.CreateFile(file);
	}



	
}