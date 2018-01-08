using Ifound.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Ifound.Models
{
    public enum Gender { male, female } ;
    //用户信息
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Pswd { get; set; }
        public string UserNo { get; set; }
        public string Tel { get; set; }
        public string Sign { get; set; }
        public string UserIcon { get; set; }
        public float BuyerPoint { get; set; }//用户得分
        public int CompleteOrderTimes { get; set; }//作为卖者完成的订单次数
        public float SailerPoint { get; set; }
        public int EvaluateTimes { get; set; }//作为卖者完成评价次数
        public Gender? Sex { get; set; }

    }

    //学生信息，认证身份用
    public class Student
    {
        [Key]
        public int Id { get; set; }
        public string StudentId { get; set; }
        public string StudentClass { get; set; }
        public string StudentName { get; set; }
    }

    public enum GoodsType { 图书, 自行车, 服装, 电子产品, 其他 };
    //所有普通二手商品
    public class Product
    {

        [Key]
        public int Id { get; set; }
        public string PdImage { get; set; }
        public string PdTitle { get; set; }
        public GoodsType? PdType { get; set; }
        public decimal? PdPrice { get; set; }
        public string PdMsg { get; set; }

        [Display(Name = "发布时间")]
        public string LaunchTime { get; set; }
        public bool IsOnShelves { get; set; }
    }

    //用户与自己发布的商品的关联表
    public class UserProduct
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PdId { get; set; }
        public int BuyerId { get; set; }
        public float? ProductScore { get; set; }
        public float? ServiceScore { get; set; }
        public float? OrderProcessingScore { get; set; }
        public float? TotalPoints { get; set; }
        public DateTime? CompleteDealTime { get; set; }
    }

    //拍卖商品表
    public class Auction
    {
        [Key]
        public int Id { get; set; }
        public string AucImage { get; set; }
        public string AucTitle { get; set; }
        public GoodsType AucType { get; set; }
        public bool IsNew{get;set;}
        //起拍价
        public decimal StartingPrice { get; set; }
        //加价幅度
        public decimal PriceIncreaseRange { get; set; }
        public string AucMsg { get; set; }
        //开始拍卖时间为发布时间
        public DateTime StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal NewPrice { get; set; }
        public int Buyerid { get; set; }
        public bool IsOnShelves { get; set; }
    }

    //拍卖商品与其发布人的关联表
    public class UserAuction
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AucId { get; set; }
        public float? ProductScore { get; set; }
        public float? ServiceScore { get; set; }
        public float? OrderProcessingScore { get; set; }
        public float? TotalPoints { get; set; }
        public string CompleteDealTime { get; set; }
        public bool Evaluated { get; set; }
    }
    //用户收藏商品表
    public class ProductWishlist
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PdId { get; set; }
        public bool ConfirmPurchase { get; set; }//确认购买
        public bool? IsTransactionSuccessful { get; set; }//交易是否成功
    }
    //用户收藏拍卖品表
    //添加字段：加价价格
    public class AuctionWishlist
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AucId { get; set; }
        public bool? IsTransactionSuccessful { get; set; }
        public decimal NewAucPrice { get; set; }
    }
    //地盘，用户上传的照片和文字，用于显示校园公共设施场所使用情况
    public class Territory
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Image { get; set; }//图片
        public string Site { get; set; }//地点
        public string Remark { get; set; }//备注，即当时想说的话、场所的使用情况..
        public string Time { get; set; }//发布时间
        public int PraiseTimes { get; set; }//被赞次数
    }
    public class Praise
    {
        [Key]
        public int Id { get; set; }
        public int TId { get; set; }
        public int UserId { get; set; }
    }

    public class IfoundDbContext : DbContext
    {
        public User user { get; set; }
        public Student student { get; set; }
        public Product product { get; set; }
        public UserProduct userproduct { get; set; }
        public Auction auction { get; set; }
        public UserAuction userauction { get; set; }
        public ProductWishlist productwishlist { get; set; }
        public AuctionWishlist auctionwishlist { get; set; }
        public Territory territory { get; set; }
        public Praise praise { get; set; }
        public System.Data.Entity.DbSet<Ifound.Models.Student> Students { get; set; }

        public System.Data.Entity.DbSet<Ifound.Models.User> Users { get; set; }

        public System.Data.Entity.DbSet<Ifound.Models.Product> Products { get; set; }

        public System.Data.Entity.DbSet<Ifound.Models.UserProduct> UserProducts { get; set; }

        public System.Data.Entity.DbSet<Ifound.Models.Auction> Auctions { get; set; }

        public System.Data.Entity.DbSet<Ifound.Models.UserAuction> UserAuctions { get; set; }

        public System.Data.Entity.DbSet<Ifound.Models.ProductWishlist> ProductWishlists { get; set; }
        public System.Data.Entity.DbSet<Ifound.Models.AuctionWishlist> AuctionWishlists { get; set; }

        public System.Data.Entity.DbSet<Ifound.Models.Territory> Territories { get; set; }
        public System.Data.Entity.DbSet<Ifound.Models.Praise> Praises { get; set; }
    }
}