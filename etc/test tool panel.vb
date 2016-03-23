
function m
	dim sql,cs,li,gtuid,accountId,results,testLinks,status,allowTestTool,showRefreshMessage, userId, orgId
	set cs = cp.csNew()
	showRefreshMessage = false 
	'
	if (cp.doc.getText( "button" ) = "enable")and(cp.doc.getText( "testValue" ) = "1234") then
		call cp.visit.setProperty( "allowTestTool", "1" )
	end if
	'
	allowTestTool = cp.visit.getBoolean( "allowTestTool" )
	if not allowTestTool then
		m = m & "<div><br>Enter 1234 to enable test tools</div>"
		m = m & "<div>" & cp.html.inputText( "testValue", "" ) & "&nbsp;" & cp.html.button( "button", "enable" ) & "</div>"
		m = cp.html.form( m )
	else
		m = ""
		results = ""
		gtuid = cp.doc.getInteger( "gtuid" )
		if gtuid<>0 then
			call cp.user.loginById( gtuid )
			results = results & "<div>Authenticating selected user...</div>"
			showRefreshMessage = true
		end if
		'
		if (cp.doc.getBoolean("testLoginCurrentUser")) then
			results = results & "<div>Authenticating you to the current user...</div>"
			call cp.user.loginById( cp.user.id )
			showRefreshMessage = true
		end if
		'
		if (cp.doc.getBoolean("testResetUsers")) then
			results = results & "<div>resetting all users, create new root user</div>"
			results = results & "<div>deleting all users...</div>"
			call cp.db.executesql( "delete from ccmembers" )
			results = results & "<div>creating new root</div>"
			userId = createUser( cp, "root", "root", "contensive", true, true )
			showRefreshMessage = true
		end if
		'
		if (cp.doc.getBoolean("testResetOrganizations")) then
			results = results & "<div>deleting all organizations</div>"
			call cp.db.executesql( "delete from organizations" )
			results = results & "<div>creating new contensive...</div>"
			orgid = createOrganization( cp, "contensive" )
		end if
		'
		if (cp.doc.getBoolean("testResetEcommerce")) then
			results = results & "<div>resetting all ecommerce to test accounts</div>"
			results = results & "<div>deleting accounts...</div>"
			call cp.db.executesql( "delete from abaccounts" )
			results = results & "<div>deleting items...</div>"
			call cp.db.executesql( "delete from items" )
			results = results & "<div>creating new data...</div>"
			call createAccount( cp, "Account-A-House", 1, 0 )
			call createAccount( cp, "Account-B-Bill-Ship-On-Payment", 3, 0 )
			call createAccount( cp, "Account-C-Bill-Ship-Now", 4, 0 )
			call createAccount( cp, "Account-D-OnDemand-With-Card", 2, 0 )
			call createItem( cp, "rock", "", 1, 0, 0 )
			call createItem( cp, "meeting", "", 2, 0, 0 )
			call createItem( cp, "subscription A, group A, weekly", "Group A", 3, 2, 7 )
		end if
		'
		if (cp.doc.getInteger("verifyEcommerceAccount")>0) then
			accountId = getAccountId()
			if ( accountId = 0 ) then
				Call cp.Doc.SetProperty("method", "createAccount")
				Call cp.Doc.SetProperty("accountName", "TestAccountUser" & cp.user.id)
				Call cp.Doc.SetProperty("userId", cp.user.id)
				accountId = cp.utils.encodeInteger( cp.Utils.ExecuteAddon("ecommerce methods") )
				call cp.db.executeSql( "update ccmembers set accountid=" & accountId & " where id=" & cp.user.id )
				results = results & "<div>Adding ecommerce account, result=[" & accountId & "]</div>"
			end if
			results = results & "<div>Verified ecommerce account, account=[" & accountId & "]</div>"
		end if
		if ( results<>"" ) then
			m = m & "<br><h3>Results</h3>" & results
			if showRefreshMessage then
				m = m & "<div>(The current page does not reflect these changes. Refresh if needed.)</div>"
			end if
		end if
		'
		' list test group users
		'
		cp.group.add( "Test Users" )
		sql = "select u.id,u.name from (( ccgroups g left join ccmemberrules r on r.groupid=g.id ) left join ccMembers u on u.id=r.memberid ) where g.name='Test Users'"
		if cs.openSql( sql ) then
			do
				li = li & "<li><a href=""?gtuid=" & cs.getText( "id" ) & """>" & cs.getText( "name" ) & "</a></li>"
				call cs.gonext()
			loop while cs.ok
		else
			li = "<li>no one in the users group</li>"
		end if
		call cs.close()
		m = m & "<ul><h3>Test Users Group</h3>" & li & "</ul>"
		'
		' status
		'
		status = ""
		if cp.user.isauthenticated then
			status = status & "<li>You are logged in as " & cp.user.name & ", id " & cp.user.id & ".</li>"
		elseif cp.user.isrecognized then
			status = status & "<li>You are recognized as " & cp.user.name & ", id " & cp.user.id & ". <a href=""?method=logout"">Logout</a>|<a href=""?method=login"">Login</a></li>"
		else
			status = status & "<li>You are a non-authenticated guest, id " & cp.user.id & ". <a href=""?method=login"">Login</a></li>"
		end if
		m = m & "<ul><h3>Status</h3>" & status & "</ul>"
		'
		' test links
		'
		testLinks = testLinks & "<li><a id=""testGoHome"" href=""/"">Public Home</a></li>"
		testLinks = testLinks & "<li><a id=""testGoAdmin"" href=""/admin"">Admin Home</a></li>"
		testLinks = testLinks & "<li><a id=""testLogout"" href=""?method=logout"">Logout</a></li>"
		testLinks = testLinks & "<li><a id=""testLoginCurrentUser"" href=""?testLoginCurrentUser=1"">Authenticate you to current user.</a></li>"
		testLinks = testLinks & "<li><a id=""testVerifyEcommerceAccount"" href=""?verifyEcommerceAccount=1"">Verify ecommerce account for this user (user.accountId)</a></li>"
		testLinks = testLinks & "<li><a id=""testResetUsers"" href=""?testResetUsers=1"">Reset users -- delete all and create new root/contensive.</a></li>"
		testLinks = testLinks & "<li><a id=""testResetOrganizations"" href=""?testResetOrganizations=1"">Reset organizations -- delete all and create new Contensive.</a></li>"
		testLinks = testLinks & "<li><a id=""testResetEcommerce"" href=""?testResetEcommerce=1"">Reset ecommerce -- delete all and reload test records.</a></li>"
		m = m & "<ul><h3>Test Links</h3>" & testLinks & "</ul>"
		'
		' assemble the body
		'
	end if
		m = "<div class=""groupLoginTool""><h2>Test Tool Panel</h2><div>(NEVER use on production sites!)</div>" & m & "</div>"
end function
'
function getAccountId()
	Call cp.Doc.SetProperty("method", "getUserAccountId")
	Call cp.Doc.SetProperty("userId", cp.user.id )
	getAccountId = cp.Utils.EncodeInteger(cp.Utils.ExecuteAddon("ecommerce methods"))
end function
'
function createUser( cp, name, username, password, admin, developer )
	dim cs, userId
	call cp.db.executesql( "delete from ccmembers where name='" & name & "'" )
	call cp.db.executesql( "delete from ccmembers where username='" & username & "'" )
	set cs = cp.csNew()
	if ( cs.insert( "people" )) then
		userId = cs.getInteger( "id" )
		call cs.setField( "name", name )
		call cs.setField( "username", username )
		call cs.setField( "password", password )
		call cs.setField( "admin", admin )
		call cs.setField( "developer", developer )
	end if
	cs.close()
	call cp.group.addUser( "Test Users", userId )
	createUser = userId
end function
'
function createOrganization( cp, name )
	dim cs, orgId
	call cp.db.executesql( "delete from organizations where name='" & name & "'" )
	set cs = cp.csNew()
	if ( cs.insert( "organizations" )) then
		orgId = cs.getInteger( "id" )
		call cs.setField( "name", name )
	end if
	cs.close()
	createOrganization = orgId
end function
'
function verifyGroup( cp, name )
	dim cs
	set cs = cp.csNew()
	if ( not cs.open( "groups", "name=" & cp.db.encodeSqlText( name ))) then
		call cs.insert( "groups" )
		call cs.setField( "name", name )
	end if
	verifyGroup = cs.getInteger( "id" )
	cs.close()
end function
'
function createAccount( cp, name, PayMethodID, membershipTypeId )
	dim cs, recordId
	recordId = 0
	call cp.db.executesql( "delete from abaccounts where name='" & name & "'" )
	set cs = cp.csNew()
	if ( cs.insert( "accounts" )) then
		recordId = cs.getInteger( "id" )
		call cs.setField( "name", name )
	end if
	call cs.setField( "PayMethodID", PayMethodID )
	call cs.setField( "membershipTypeId", membershipTypeId )
	cs.close()
	createAccount = recordId
end function
'
function createItem( cp, name, groupName, unitPrice, membershipDurationTypeId, GroupExpirationPeriod  )
	dim cs, recordId, groupId
	recordId = 0
	set cs = cp.csNew()
	call cp.db.executesql( "delete from items where name='" & name & "'" )
	if ( cs.insert( "items" )) then
		recordId = cs.getInteger( "id" )
		call cs.setField( "name", name )
		call cs.setField( "unitPrice", unitPrice )
		call cs.setField( "membershipDurationTypeId", membershipDurationTypeId )
		call cs.setField( "GroupExpirationPeriod", GroupExpirationPeriod )
	end if
	if groupName<>"" then
		call cs.setField( "groupId", verifyGroup( cp, groupName ))
	end if
	cs.close()
	createItem = recordId
end function
'
sub changeItemtoMembership( cp, itemId )
	call cp.db.executeSql( "update items set contentcontrolid=" & cp.content.getId( "membership types" ) & " where id=" & itemId )
end sub


