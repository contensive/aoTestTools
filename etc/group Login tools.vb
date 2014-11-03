function m
	dim sql,cs,li,gtuid
	gtuid = cp.doc.getInteger( "gtuid" )
	if gtuid<>0 then
		call cp.user.loginById( gtuid )
	end if
	set cs = cp.csNew()
	if cp.user.isauthenticated then
		m = m & "<div>You are logged in as " & cp.user.name & ", id " & cp.user.id & ". <a href=""?method=logout"">Logout</a></div>"
		m = m & "<div><a href=""/admin"">Admin Home</a></div>"
	elseif cp.user.isrecognized then
		m = m & "<div>You are recognized as " & cp.user.name & ", id " & cp.user.id & ". <a href=""?method=logout"">Logout</a>|<a href=""?method=login"">Login</a></div>"
	else
		m = m & "<div>You are not authenticated. <a href=""?method=login"">Login</a></div>"
	end if
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
	m = "<div class=""groupLoginTool""><h2>Group Login Tool</h2>" & m & "</div>"
end function