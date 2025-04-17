using System;

namespace chakish_2._0_api.Models;

public class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
    
}