
function m
	dim sql,cs,li,gtuid,accountId,results,testLinks,status,allowTestTool,showRefreshMessage, userId, orgId,TestToolPassword
	dim toolPanelTestId,toolPanelTestName,rqs, toolPanelTestAddonid
	'
	set cs = cp.csNew()
	showRefreshMessage = false 
	rqs = cp.doc.refreshQueryString
	'
	allowTestTool = cp.visit.getBoolean( "allowTestTool" )
	if not allowTestTool then
		TestToolPassword = cp.file.read( cp.site.PhysicalInstallPath & "\config\TestToolPassword.txt")
		if ( TestToolPassword = "" ) then
			m = m & "The TestToolPassword.txt is missing or empty. If this is a test site, ask a developer to populate the password in the file [config\TestToolPassword.txt] in the Contensive installation folder."
		else
			if (cp.doc.getText( "button" ) = "enable")  then
				if (cp.doc.getText( "TestToolPassword" ) = TestToolPassword ) then
					allowTestTool = true
					call cp.visit.setProperty( "allowTestTool", "1" )
				else
					m = m & "The password you entered is not correct. The password should be stored in the file [config\TestToolPassword.txt] in the Contensive installation folder."
				end if
			end if
		end if
		m = m & "<div><br>Enter the TestToolPassword to enable test tools</div>"
		m = m & "<div>" & cp.html.inputText( "TestToolPassword", "" ) & "&nbsp;" & cp.html.button( "button", "enable" ) & "</div>"
		m = cp.html.form( m )
	end if
	if allowTestTool then
		m = ""
		results = ""
		'
		' run tool panel tests
		'
		toolPanelTestId = cp.doc.getInteger( "toolPanelTestId" )
		if (toolPanelTestId>0) then
			if cs.open( "Tool Panel Tests", "id=" & toolPanelTestId ) then
				toolPanelTestAddonid = cs.getInteger( "addonid" )
				toolPanelTestAddonName = cs.getText( "name" )
				if ( toolPanelTestAddonid>0 ) then
					call cp.doc.setproperty( "runFromToolPanel", "1" )
					results = results & "<div>Result=" & cp.utils.executeAddon( toolPanelTestAddonId ) & "</div>"
					call cp.doc.setproperty( "runFromToolPanel", "0" )
				end if
			end if
			call cs.close()
		end if
		'
		' old hardcoded tests ----- to be converted to tool panel tests
		'
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
		' display test results
		'
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
		' test links ----- to be converted to tool panel tests
		'
		testLinks = testLinks & "<li><a id=""testGoHome"" href=""/"">Public Home</a></li>"
		testLinks = testLinks & "<li><a id=""testGoAdmin"" href=""/admin"">Admin Home</a></li>"
		testLinks = testLinks & "<li><a id=""testLogout"" href=""?method=logout"">Logout</a></li>"
		testLinks = testLinks & "<li><a id=""testLoginCurrentUser"" href=""?testLoginCurrentUser=1"">Authenticate you to current user.</a></li>"
		testLinks = testLinks & "<li><a id=""testVerifyEcommerceAccount"" href=""?verifyEcommerceAccount=1"">Verify ecommerce account for this user (user.accountId)</a></li>"
		testLinks = testLinks & "<li><a id=""testResetUsers"" href=""?testResetUsers=1"">Reset users -- delete all people and create a new root/contensive.</a></li>"
		testLinks = testLinks & "<li><a id=""testResetOrganizations"" href=""?testResetOrganizations=1"">Reset organizations -- delete all organizations and create new Contensive.</a></li>"
		'
		' list tool panel tests
		'
		if ( cs.open( "tool panel tests" )) then
			do
				toolPanelTestId = cs.getInteger( "id" )
				toolPanelTestName = cs.getText( "name" )
				testLinks = testLinks & "<li><a id=""toolPanelTestId" & toolPanelTestId & """ href=""?" & rqs & "&toolPanelTestId=" & toolPanelTestId & """>" & toolPanelTestName & "</a></li>"
				cs.gonext()
			loop while cs.ok()
		end if
		call cs.close()
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


