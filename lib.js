function loadModal(sender){
    console.log("Good");
    $("#modal").css("visibility","visible");
    $("#modalImg").attr("src",sender.src);
  }
  function closeModal(sender){
    $("#modal").css("visibility","hidden");
  }