// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.



// Write your JavaScript code.

const accordionItemHeaders = document.querySelectorAll(".accordion-item-header");

accordionItemHeaders.forEach(accordionItemHeader => {
    accordionItemHeader.addEventListener("click", event => {

        // Uncomment in case you only want to allow for the display of only one collapsed item at a time!

         const currentlyActiveAccordionItemHeader = document.querySelector(".accordion-item-header.active");
         if(currentlyActiveAccordionItemHeader && currentlyActiveAccordionItemHeader!==accordionItemHeader) {
           currentlyActiveAccordionItemHeader.classList.toggle("active");
           currentlyActiveAccordionItemHeader.nextElementSibling.style.maxHeight = 0;
         }

        accordionItemHeader.classList.toggle("active");
        const accordionItemBody = accordionItemHeader.nextElementSibling;
        if (accordionItemHeader.classList.contains("active")) {
            accordionItemBody.style.maxHeight = accordionItemBody.scrollHeight + "px";
        }
        else {
            accordionItemBody.style.maxHeight = 0;
        }

    });
});


function AccordianChange(id, count) {
    var str = "collapse" + id;
    var ishidden = document.getElementById(str).hidden;
    if (ishidden) {
        document.getElementById(str).hidden = false;
    }
    else {
        document.getElementById(str).hidden = true;
    }

    for (var i = 1; i <= count; i++) {
        if (i != id) {
            str = "collapse" + i;
            document.getElementById(str).hidden = true;
        }
    }

}

function MinusClickCart(id) {
    var str = "#ProductQty" + id;
    var count = $(str).val() - 1;
    count = count < 1 ? 1 : count;
    $(str).val(count);
    $(str).change();
    return false;
}
function PlusClickCart(id) {
    var str = "#ProductQty" + id;
    var count = parseInt($(str).val());
    count = count + 1;
    $(str).val(count);
    $(str).change();
    return false;
}

function EditCartQty(cartid, saleprice) {
    var prdstr = "#ProductQty" + cartid;
    salestr = "#SalePrice" + cartid;
    var qty = parseInt($(prdstr).val());  
    
    var txt = $('#Subtotal').text();
    txt = txt.substring(1);
    var SubTotal = parseInt(txt);

    var txt = $(salestr).text()
    txt = txt.substring(1);
    var currSalePrice = parseInt(txt);

    if (qty > 0) {
        var url = "/Home/EditCartQuantity?CartId=" + cartid + "&Quantity=" + qty;
        $.get(url).done(function (data) {
            var count = saleprice * qty;
            $(salestr).text('$' + count);
            $(salestr).change();
            if (currSalePrice > count) {
                SubTotal = SubTotal - (currSalePrice - count);
            }
            else {
                SubTotal = SubTotal + (count - currSalePrice);
            }
            $('#Subtotal').text('$' + SubTotal);
            $('#Subtotal').change();
            return false;
        }).fail(function (data) {
            if (data.status == 401) {
                window.location.href = "/Identity/Account/Login?ReturnUrl=%2F" + page;
            }
            else {
                $.notify(data.responseText, { globalPosition: 'bottom left', className: 'danger' });
            }
        });
    }
}

function EditAddressType(str, addId) {
    var url = "/Home/SetAs" + str +"?addressId=" + addId;
    $.get(url).done(function (data) {
        location.reload();
    }).fail(function (data) {
        if (data.status == 401) {
            window.location.href = "/Identity/Account/Login?ReturnUrl=%2F" + page;
        }
        else {
            $.notify(data.responseText, { globalPosition: 'bottom left', className: 'danger' });
        }
    });
}


function RemoveFromCart(cartid) {
    var url = "/Home/RemoveFromCart";
    url = CreateURL(url, { 'CartId': cartid })
    $.get(url).done(function (data) {
        location.reload();
    }).fail(function (data) {
        if (data.status == 401) {
            window.location.href = "/Identity/Account/Login?ReturnUrl=%2F" + page;
        }
        else {
            $.notify(data.responseText, { globalPosition: 'bottom left', className: 'danger' });
        }
    });
}

function RemoveAddress(addressid) {
    var url = "/Home/RemoveAddress";
    url = CreateURL(url, { 'AddressId': addressid })
    $.get(url).done(function (data) {
        location.reload();
    }).fail(function (data) {
        if (data.status == 401) {
            window.location.href = "/Identity/Account/Login?ReturnUrl=%2F" + page;
        }
        else {
            $.notify(data.responseText, { globalPosition: 'bottom left', className: 'danger' });
        }
    });
}

function SelectCard(cardId, count) {
    $('#inputSelectCardNum').val(cardId);
    $('#inputSelectCardNum').change();

    var idstr = "cardSelect" + cardId;

    document.getElementById(idstr).style.border = 'dashed';
    document.getElementById(idstr).style.borderColor = '#96672d'
    for (var i = 0; i < count; i++) {
        if (i != cardId) {
            idstr = "cardSelect" + i;
            document.getElementById(idstr).style.border = 'none';
            document.getElementById(idstr).style.borderColor = ''
        }
    }
}

function PlaceOrder(productId, qty) {
    document.getElementById('placeorderbtn').hidden = true;
    document.getElementById('processorderbtn').hidden = false;
    var url = "/Home/PlaceOrder";
    var cardId = $('#inputSelectCardNum').val();
    if (cardId < 0) {
        document.getElementById('placeorderbtn').hidden = false;
        document.getElementById('processorderbtn').hidden = true;
        $.notify("Please Select a Card", { globalPosition: 'bottom left', className: 'danger' });
        return;
    }
    url = CreateURL(url, { 'cardId': cardId })
    if (productId != null && productId != undefined && qty != null && qty != undefined && qty >= 1) {
        url = CreateURL(url, { 'productId': productId, 'qty': qty })
    }
    $.get(url).done(function (data) {
        window.location.href = "/Home/MyOrders";
    }).fail(function (data) {
        if (data.status == 401) {
            window.location.href = "/Identity/Account/Login?ReturnUrl=%2F" + page;
        }
        else {
            document.getElementById('placeorderbtn').hidden = false;
            document.getElementById('processorderbtn').hidden = true;
            $.notify(data.responseText, { globalPosition: 'bottom left', className: 'danger' });
        }
    });
}

function RemoveCard(cardid) {
    var url = "/Home/RemoveCard";
    url = CreateURL(url, { 'CardId': cardid })
    $.get(url).done(function (data) {
        location.reload();
    }).fail(function (data) {
        if (data.status == 401) {
            window.location.href = "/Identity/Account/Login?ReturnUrl=%2F" + page;
        }
        else {
            $.notify(data.responseText, { globalPosition: 'bottom left', className: 'danger' });
        }
    });
}

const rangeInput = document.querySelectorAll(".range-input input"),
priceInput = document.querySelectorAll(".price-input input"),
range = document.querySelector(".slider .progress");
let priceGap = 1;
priceInput.forEach(input => {
    input.addEventListener("input", e => {
        let minPrice = parseInt(priceInput[0].value),
            maxPrice = parseInt(priceInput[1].value);

        if ((maxPrice - minPrice >= priceGap) && maxPrice <= rangeInput[1].max) {
            if (e.target.className === "input-min") {
                rangeInput[0].value = minPrice;
                range.style.left = ((minPrice / rangeInput[0].max) * 100) + "%";
            } else {
                rangeInput[1].value = maxPrice;
                range.style.right = 100 - (maxPrice / rangeInput[1].max) * 100 + "%";
            }
        }
    });
});
rangeInput.forEach(input => {
    input.addEventListener("input", e => {
        let minVal = parseInt(rangeInput[0].value),
            maxVal = parseInt(rangeInput[1].value);
        if ((maxVal - minVal) < priceGap) {
            if (e.target.className === "range-min") {
                rangeInput[0].value = maxVal - priceGap
            } else {
                rangeInput[1].value = minVal + priceGap;
            }
        } else {
            priceInput[0].value = minVal;
            priceInput[1].value = maxVal;
            range.style.left = ((minVal / rangeInput[0].max) * 100) + "%";
            range.style.right = 100 - (maxVal / rangeInput[1].max) * 100 + "%";
        }
    });
});

function ChangeClick(stri, str) {
    var url = "/Product/" + stri + "?barcode=" + str;
    $.get(url).done(function (data) {
        location.reload();
    }).fail(function (data) {
        $.notify(data.responseText, { globalPosition: 'bottom left', className: 'danger' });
    });
}

function BuyNow(prodId, qty) {
    var url = "/Home/Checkout";
    url = CreateURL(url, { 'ProductId': prodId, 'Quantity': qty })

    window.location.href = url;
}

function AddtoCart(prodId, qty, page) {
    var url = "/Home/AddtoCart";
    url = CreateURL(url, { 'ProductId': prodId, 'Quantity': qty })
    $.get(url).done(function (data) {
        $.notify(data, { globalPosition: 'bottom left', className: 'success' });
    }).fail(function (data) {
        if (data.status == 401) {
            window.location.href = "/Identity/Account/Login?ReturnUrl=%2F" + page;
        }
        else{
            $.notify(data.responseText, { globalPosition: 'bottom left', className: 'danger' });
        }
    });
}


function CreateURL(url, list) {
    var subString = "";
    if (!url.includes('?')) {
        url += '?';
    }
    else {
        var index = url.indexOf('?');
        subString = url.substring(index + 1);
        url = url.substring(0, index + 1);
    }
    const SearchUrl = new URLSearchParams(subString);
    for (var key in list) {
        var property = list[key];
        SearchUrl.set(key, property);
    }

    return url + SearchUrl.toString();
}


function GetSubCat() {
    var url = "/Product/GetSubCatList?CatID=" + $('#acategory').val();
    $.get(url).done(function (data) {
        var subcat = '';
        $("#asubcategory").empty();

        $.each(data, function (i, subcategory) {
            subcat += "<option value=" + subcategory.value + ">" + subcategory.text + "</options>";
        });
        $('#asubcategory').html(subcat);
    });
}

function CancelOrder(orderId) {
    var url = "/Home/CancelOrder";
    var reason = $('#CancelReason').val();
    url = CreateURL(url, { 'orderId': orderId, 'Reason': reason })
    $.get(url).done(function (data) {
        location.reload();
    }).fail(function (data) {
        if (data.status == 401) {
            window.location.href = "/Identity/Account/Login?ReturnUrl=%2F" + page;
        }
        else {
            $.notify(data.responseText, { globalPosition: 'bottom left', className: 'danger' });
        }
    });
}

function OpenUploadPage() {
    window.location = "/Product/UploadDataGet";
}


$(function () {
    var placeholderElement = $('#placeholder');
    $('button[data-toggle="ajax-modal"]').click(function (event) {
        var url = $(this).data('url');
        $.get(url).done(function (data) {
            placeholderElement.html(data);
            placeholderElement.find('.modal').modal('show');
        });
    });

    $("#fliterbtnsearch").click(function (event) {
        var url = window.location.href;

        window.location.href = CreateURL(url, {
            "CategoryID": $('#category').val(),
            "SubCategoryId": $('#subcategory').val(),
            "Search": $('#search').val(),
            "Min": $('#min').val(),
            "Max": $('#max').val(),
            "pagefrom": 1,
            "pagetotal": $('#rowselectId').val()

        });
    });

    $('#fliterProductsSearch').click(function (event) {
        var url = window.location.href;

        window.location.href = CreateURL(url, {
            "CategoryID": $('#category').val(),
            "SubCategoryId": $('#subcategory').val(),
            "Search": $('#search').val(),
            "pagefrom": 1,
            "pagetotal": $('#rowselectId').val()

        });
    });


    $("#fliterbtnsearchUser").click(function (event) {
        var url = window.location.href;

        window.location.href = CreateURL(url, {
            "StateID": $('#stateList').val(),
            "search": $('#searchUser').val(),
            "UserRole": $('#userRoles').val(),
            "pagefrom": 1,
            "pagetotal": $('#rowselectId').val()

        });
    });

    $("#prebtnClick").click(function (event) {
        var url = window.location.href;
        var currentpage = $("#cuttentpageid").val();

        if (currentpage > 1) {
            currentpage--;

            window.location.href = CreateURL(url, { "pagefrom": currentpage });
        }
    });

    $("#featurebtn").click(function (event) {
        var url = window.location.href;
        var type = $("#type").val();

        if (type.includes('featured')) {
            type = "all";
        } else {
            type = 'featured';
        }
        window.location.href = CreateURL(url, { "type": type });
        
    });

    $('#popularbtn').click(function (event) {
        var url = window.location.href;
        var type = $("#type").val();

        if (type.includes('popular')) { 
            type = "all";
        } else {
            type = 'popular';
        }
        window.location.href = CreateURL(url, { "type": type });

    });

    $("#nextbtnClick").click(function (event) {
        var url = window.location.href;
        var currentpage = parseInt($("#cuttentpageid").val());
        var totalpage = parseInt($("#totalpageid").val());

        if (currentpage < totalpage) {
            currentpage++;
            url = CreateURL(url, { "pagefrom": currentpage });
            window.location.href = url;
        }
    });

    $("#rowselectId").change(function (event) {
        var url = window.location.href;

        window.location.href = CreateURL(url, { "pagetotal": $('#rowselectId').val() });
    });

    $('#category').change(function (event) {
        var url = "/Product/GetSubCatList?CatID=" + $('#category').val();
        $.get(url).done(function (data) {
               var subcat = '';
            $("#subcategory").empty();

            $.each(data, function (i, subcategory) {
                subcat += "<option value=" + subcategory.value + ">" + subcategory.text + "</options>";
            });
            $('#subcategory').html(subcat);
        });
    });

    $("#salePriceClick").click(function (event) {
        var url = window.location.href;
        var sort = $('#sortbySaleOrder').val();
        if (sort == 1) {
            sort = 0;
        }
        else {
            sort = 1;
        }
        window.location.href = CreateURL(url, { "sortby": 3, "sortbyorder": sort });
    });

    $("#QtyPriceClick").click(function (event) {
        var url = window.location.href;
        var sort = $('#sortbyQtyOrder').val();
        if (sort == 1) {
            sort = 0;
        }
        else {
            sort = 1;
        }
        window.location.href = CreateURL(url, { "sortby": 1, "sortbyorder": sort });
    });


    $("#BasePriceClick").click(function (event) {
        var url = window.location.href;
        var sort = $('#sortbyBaseOrder').val();
        if (sort == 1) {
            sort = 0;
        }
        else {
            sort = 1;
        }
        window.location.href = CreateURL(url, { "sortby": 2, "sortbyorder": sort });
    });

    placeholderElement.on('click', '[data-save="modal"]', function (event) {

        var form = $(this).parents('.modal').find('form');
        var actionUrl = form.attr('action');
        var sendData = form.serialize();
        $.post(actionUrl, sendData).done(function (data) {
            placeholderElement.find('.modal').modal('hide');
            location.reload();
        }).fail(function (data) {
            $.notify(data.responseText, { globalPosition: 'bottom left', className: 'danger' });
        });
    });
});
