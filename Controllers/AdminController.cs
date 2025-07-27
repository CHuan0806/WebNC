
using Microsoft.AspNetCore.Mvc;
using QLNhaSach1.Models;
using QLNhaSach1.Service;
using System.Linq;
using System;
using System.Collections.Generic;

public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly CacheService _cacheService;

    public AdminController(AppDbContext context, CacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public IActionResult Index()
    {
        // Dữ liệu tổng quan
        ViewBag.TotalBooks = _context.Books.Count();
        ViewBag.TotalOrders = _context.Orders.Count();
        ViewBag.TotalRevenue = _context.Orders.Sum(o => o.TotalPrice);
        ViewBag.TotalUsers = _context.Users.Count();
        

        // Dữ liệu doanh thu theo tháng (6 tháng gần nhất)
        var revenueData = GetRevenueByMonth();
        ViewBag.RevenueLabels = revenueData.Select(x => x.Month).ToList();
        ViewBag.RevenueData = revenueData.Select(x => x.Revenue).ToList();

        // Dữ liệu tỷ lệ đơn hàng theo trạng thái
        var orderStatusData = GetOrderStatusDistribution();
        ViewBag.OrderStatusLabels = orderStatusData.Select(x => x.Status).ToList();
        ViewBag.OrderStatusData = orderStatusData.Select(x => x.Count).ToList();

        return View();
    }

    private List<RevenueByMonth> GetRevenueByMonth()
    {
        var result = new List<RevenueByMonth>();
        var currentDate = DateTime.Now;
        
        for (int i = 5; i >= 0; i--)
        {
            var month = currentDate.AddMonths(-i);
            var monthName = month.ToString("MMMM yyyy", new System.Globalization.CultureInfo("vi-VN"));
            
            var revenue = _context.Orders
                .Where(o => o.OrderDate.Month == month.Month && o.OrderDate.Year == month.Year)
                .Sum(o => o.TotalPrice);
            
            result.Add(new RevenueByMonth { Month = monthName, Revenue = revenue });
        }
        
        return result;
    }

    private List<OrderStatusCount> GetOrderStatusDistribution()
    {
        // Lấy dữ liệu thô từ DB
        var result = _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusCount 
            { 
                Status = g.Key, 
                Count = g.Count() 
            })
            .ToList();

        // Chuyển đổi trạng thái sang tiếng Việt ở ngoài DB
        foreach (var item in result)
        {
            item.Status = GetStatusDisplayName(item.Status);
        }

        return result;
    }

        private string GetStatusDisplayName(string status)
    {
        return status switch
        {
            "Pending" => "Chờ xử lý",
            "Processing" => "Đang giao",
            "Completed" => "Hoàn thành",
            "Cancelled" => "Đã hủy",
            _ => status
        };
    }

    [HttpGet]
    public IActionResult MultiChat()
    {
        // Lấy danh sách tất cả người dùng không phải admin để chat
        var users = _context.Users
            .Where(u => u.Role == Role.User)
            .Select(u => new { u.UserId, u.UserName, u.Email })
            .ToList();

        ViewBag.Users = users;
        return View();
    }
}

public class RevenueByMonth
{
    public string Month { get; set; }
    public decimal Revenue { get; set; }
}

public class OrderStatusCount
{
    public string Status { get; set; }
    public int Count { get; set; }
}